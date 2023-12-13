Imports System.IO
Imports System.Threading
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IMonitor
    Function RunAsync() As Task(Of IOperationResult)
End Interface

Public Class FileMonitor
    Implements IMonitor

    Private ReadOnly _ui As IUserInteraction
    Private ReadOnly _utility As IUtility
    Private ReadOnly _fileSync As IFileSync

    ' Directory to monitor - this should be replaced with your actual directory path
    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of FileMonitor)

    ' ----------------- Debounce -----------------
    Private _lastReadTimes As New Dictionary(Of String, DateTime)
    Private _debounceTime As TimeSpan = TimeSpan.FromMilliseconds(200) ' 200 milliseconds
    ' --------------- End Debounce ---------------

    Public Sub New(ui As IUserInteraction, utility As IUtility, fileSync As IFileSync, options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of FileMonitor))
        _ui = ui
        _utility = utility
        _fileSync = fileSync
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    Public Async Function RunAsync() As Task(Of IOperationResult) Implements IMonitor.RunAsync
        Dim result = OperationResult.Ok

        Dim monitorDirectories = _utility.ReadTConfig()

        ' Filter out subdirectories that are already covered
        monitorDirectories = _utility.FilterRedundantDirectoriesFromList(monitorDirectories)

        ' Filter out the application directory
        monitorDirectories = _utility.FilterInstallDirFromList(monitorDirectories)

        ' Catch empty list
        If monitorDirectories.Count = 0 Then
            _logger.LogWarning("{Method}: No root paths found in the .tconfig file.", NameOf(RunAsync))
            Return OperationResult.Fail
        End If

        Dim cts As New CancellationTokenSource()

        Dim monitorTasks As New List(Of Task)

        For Each directory In monitorDirectories
            Try
                monitorTasks.Add(Task.Run(Sub() Monitor(directory, cts.Token)))
            Catch ex As Exception
                _logger.LogError(ex, "{Method}: Error adding task for directory: {Directory}", NameOf(RunAsync), directory)
            End Try
        Next

        Do Until monitorTasks.All(Function(t) t.IsCompleted)

            If _ui.ExitApplication() Then
                cts.Cancel()
            End If

            ' Sleep to avoid pegging the CPU
            System.Threading.Thread.Sleep(500)
        Loop

        Await Task.WhenAll(monitorTasks.ToArray)

        Return result

    End Function

    ' Filter out the application directory
    'Private Function FilterAppDir(monitorDirectories As List(Of String)) As List(Of String)

    '    Dim appDir As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

    '    Return monitorDirectories.Where(Function(dir) Not dir.Equals(appDir, StringComparison.OrdinalIgnoreCase)).ToList()

    'End Function

    Private Sub Monitor(directory As String, cancelToken As CancellationToken)
        ' Directory to monitor - this should be replaced with your actual directory path


        Using watcher As New FileSystemWatcher(directory)

            ' Watch for changes in LastWrite times, and the renaming of files or directories.
            watcher.IncludeSubdirectories = True

            ' Watch for changes in LastWrite times, and the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.FileName Or NotifyFilters.DirectoryName

            ' Only watch specific file types
            watcher.Filter = "*.*" ' Modify this to target specific extensions

            ' Add event handlers.
            AddHandler watcher.Changed, AddressOf OnChanged
            AddHandler watcher.Created, AddressOf OnChanged
            AddHandler watcher.Deleted, AddressOf OnChanged
            AddHandler watcher.Renamed, AddressOf OnRenamed

            ' Begin watching.
            watcher.EnableRaisingEvents = True

            While Not cancelToken.IsCancellationRequested
                System.Threading.Thread.Sleep(1000)
            End While

        End Using
    End Sub

    Private Sub OnChanged(sender As Object, e As FileSystemEventArgs)

        ' ----------------- Debounce -----------------

        Dim lastReadTime As DateTime
        Dim currentTime As DateTime = DateTime.Now

        SyncLock _lastReadTimes
            If _lastReadTimes.TryGetValue(e.FullPath, lastReadTime) Then
                If currentTime - lastReadTime < _debounceTime Then
                    ' This event is within the debounce time, ignore it.
                    Return
                End If
            End If
            ' Update the last read time.
            _lastReadTimes(e.FullPath) = currentTime
        End SyncLock
        ' --------------- End debounce -----------------

        Dim isFileOfInterest As Boolean = _fileSync.IsFileOfInterest(e.FullPath)
        Dim isFileInAppDir As Boolean '= _utility.IsInstallDir(Path.GetDirectoryName(e.FullPath))
        Dim isFileInTransclude As Boolean '= _utility.ExistsFileInTransclude(e.FullPath)

        ' Check if the file is of interest first to avoid proccessing ephemeral files/directories
        ' that may be deleted before the method completes
        If isFileOfInterest Then
            Try
                isFileInAppDir = _utility.IsInstallDir(Path.GetDirectoryName(e.FullPath))
                isFileInTransclude = _utility.ExistsFileInTransclude(e.FullPath)
            Catch ex As Exception
                _logger.LogWarning(ex, "{Method}: Error checking if file is in application directory or .transclude file: {FullPath}", NameOf(OnChanged), e.FullPath)
                Return
            End Try

        End If

        ' Check if the file is of interest, is in the .transclude file, and is not in the application directory
        If isFileOfInterest AndAlso isFileInTransclude AndAlso Not isFileInAppDir Then

            ' Sync the source file to the Markdown file
            _fileSync.SyncSourceToMarkdown(e.FullPath)

            _logger.LogInformation("{Method}: File of interest changed: {FullPath} {ChangeType}", NameOf(OnChanged), e.FullPath, e.ChangeType)
        End If
    End Sub

    Private Sub OnRenamed(sender As Object, e As RenamedEventArgs)

        Dim isFileOfInterest As Boolean = _fileSync.IsFileOfInterest(e.FullPath)
        Dim isFileInAppDir As Boolean '= _utility.IsInstallDir(Path.GetDirectoryName(e.FullPath))
        Dim isOldFileInTransclude As Boolean '= _utility.ExistsFileInTransclude(e.OldFullPath)

        ' Check if the file is of interest first to avoid proccessing ephemeral files/directories
        ' that may be deleted before the method completes
        If isFileOfInterest Then
            Try
                isFileInAppDir = _utility.IsInstallDir(Path.GetDirectoryName(e.FullPath))
                isOldFileInTransclude = _utility.ExistsFileInTransclude(e.OldFullPath)
            Catch ex As Exception
                _logger.LogWarning(ex, "{Method}: Error checking if file is in application directory or .transclude file: {FullPath}", NameOf(OnRenamed), e.FullPath)
                Return
            End Try
        End If


        ' Check if the file is of interest, is in the .transclude file, and is not in the application directory
        If isFileOfInterest AndAlso isOldFileInTransclude AndAlso Not isFileInAppDir Then

            ' Implement your sync logic here
            _logger.LogInformation("{Method}: File: {OldFullPath} renamed to {FullPath}", NameOf(OnRenamed), e.OldFullPath, e.FullPath)

            ' Add the new file to the .transclude file
            _utility.AddSpecificFileToTransclude(e.FullPath)

            ' Sync the source file to the Markdown file
            _fileSync.SyncSourceToMarkdown(e.FullPath)

            ' Mark the old md file as stale
            _fileSync.MarkFileAsStale(e.OldFullPath)

        End If

    End Sub

End Class
