Imports System.IO
Imports System.Threading
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports Newtonsoft.Json.Linq
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

    Private _monitorDirectory As String = "C:\source\repos\TransCodeMD\demo"

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

        Dim monitorDirectories = _utility.ReadMonitorDirectories()

        Dim cts As New CancellationTokenSource()

        'Dim monitorTask = Task.Run(Sub() Monitor(cts.Token))

        Dim monitorTasks As New List(Of Task)

        For Each directory In monitorDirectories
            Try
                monitorTasks.Add(Task.Run(Sub() Monitor(cts.Token)))
            Catch ex As Exception
                _logger.LogError(ex, "{Method}: Error adding task for directory: {Directory}", NameOf(RunAsync), directory)
            End Try
        Next

        Do Until monitorTasks.All(Function(t) t.IsCompleted)
            'If System.Console.ReadKey().KeyChar = "q"c Then
            '    cts.Cancel()
            'End If

            If _ui.ExitApplication() Then
                cts.Cancel()
            End If

            ' Sleep to avoid pegging the CPU
            System.Threading.Thread.Sleep(500)
        Loop

        'Do While Not monitorTask.IsCompleted
        '    'If System.Console.ReadKey().KeyChar = "q"c Then
        '    '    cts.Cancel()
        '    'End If

        '    If _ui.ExitApplication() Then
        '        cts.Cancel()
        '    End If

        '    ' Sleep to avoid pegging the CPU
        '    System.Threading.Thread.Sleep(500)
        'Loop

        'Await Task.Run(Sub() Monitor(cts.Token))

        Await Task.WhenAll(monitorTasks.ToArray)

        Return result

    End Function

    Private Sub Monitor(cancelToken As CancellationToken)
        ' Directory to monitor - this should be replaced with your actual directory path


        Using watcher As New FileSystemWatcher(_monitorDirectory)
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

            ' Wait for the user to quit the program.
            '_logger.LogInformation("Press 'q' to quit the sample.")


            'While Char.ToLowerInvariant(System.Console.ReadKey().KeyChar) <> "q"c
            '    System.Threading.Thread.Sleep(1000)
            'End While

            While Not cancelToken.IsCancellationRequested
                System.Threading.Thread.Sleep(1000)
            End While

        End Using
    End Sub

    Private Sub OnChanged(sender As Object, e As FileSystemEventArgs)

        If _fileSync.IsFileOfInterest(e.FullPath) Then
            ' Implement your sync logic here
            'System.Console.WriteLine($"File of interest changed: {e.FullPath} {e.ChangeType}")
            _fileSync.SyncSourceToMarkdown(e.FullPath)
            _logger.LogInformation("{Method}: File of interest changed: {FullPath} {ChangeType}", NameOf(OnChanged), e.FullPath, e.ChangeType)
        End If
    End Sub

    Private Sub OnRenamed(sender As Object, e As RenamedEventArgs)

        If _fileSync.IsFileOfInterest(e.FullPath) Then
            ' Implement your sync logic here
            _logger.LogInformation("{Method}: File: {OldFullPath} renamed to {FullPath}", NameOf(OnRenamed), e.OldFullPath, e.FullPath)

            ' Add the new file to the .transclude file
            _utility.AddFilesToTransclude(Path.GetDirectoryName(e.FullPath))

            ' Sync the source file to the Markdown file
            _fileSync.SyncSourceToMarkdown(e.FullPath)

            ' Mark the old md file as stale
            _fileSync.MarkFileAsStale(e.OldFullPath)

        End If

    End Sub

End Class
