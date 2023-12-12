Imports System.IO
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUtility
    Sub AddFilesToTransclude(directoryPath As String, Optional specificFilePath As String = "")
    Function GetFilesToTransclude(directoryPath As String) As List(Of String)
    Sub CreateTranscludeFile(directoryPath As String)
    Function ReadMonitorDirectories() As List(Of String)
    Sub AddDirectoryToConfig(Optional directoryPath As String = Nothing)
    Function FilterAppDir(monitorDirectories As List(Of String)) As List(Of String)
    Function FilterRedundantDirectories(directories As List(Of String)) As List(Of String)
    Sub ManualSync(Optional directory As String = Nothing)
End Interface

Public Class Utility
    Implements IUtility

    Private ReadOnly _fileSync As IFileSync
    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of Utility)

    Public Sub New(fileSync As IFileSync, options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of Utility))
        _fileSync = fileSync
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    ' Create a .transclude file in the directory if it doesn't already exist
    Public Sub CreateTranscludeFile(directoryPath As String) Implements IUtility.CreateTranscludeFile
        Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")

        ' Check if the file already exists
        If Not File.Exists(transcludeFilePath) Then
            ' Create an empty .transclude file
            File.WriteAllText(transcludeFilePath, String.Empty)
            'Console.WriteLine($"Created new .transclude file at: {transcludeFilePath}")
            _logger.LogInformation("{Method}: Created new .transclude file at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
        Else
            'Console.WriteLine($"A .transclude file already exists at: {transcludeFilePath}")
            _logger.LogInformation("{Method}: A .transclude file already exists at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
        End If
    End Sub

    ' Read the .transclude file and return a list of all files that should be transcluded
    Public Function GetFilesToTransclude(directoryPath As String) As List(Of String) Implements IUtility.GetFilesToTransclude
        Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
        If Not File.Exists(transcludeFilePath) Then
            Return New List(Of String)()
        End If

        ' Read all lines from the .transclude file
        Dim lines As String() = File.ReadAllLines(transcludeFilePath)
        Return lines.ToList()
    End Function

    ' Add all files in the directory to the .transclude file that aren't already in it and are "of interest"
    'Public Sub AddFilesToTransclude(directoryPath As String) Implements IUtility.AddFilesToTransclude
    '    Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
    '    Dim filesToTransclude As List(Of String) = GetFilesToTransclude(directoryPath)

    '    ' Get all files in the directory
    '    Dim filesInDirectory As String() = Directory.GetFiles(directoryPath)

    '    ' Add all files that aren't already in the .transclude file and are "of interest"
    '    For Each filePath As String In filesInDirectory
    '        If _fileSync.IsFileOfInterest(filePath) AndAlso Not filesToTransclude.Contains(filePath) Then
    '            filesToTransclude.Add(filePath)
    '        End If
    '    Next

    '    ' Write the updated list of files to the .transclude file
    '    File.WriteAllLines(transcludeFilePath, filesToTransclude)
    'End Sub

    Public Function ReadMonitorDirectories() As List(Of String) Implements IUtility.ReadMonitorDirectories
        Dim configFilePath As String = GetConfigFilePath() 'Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tconfig")
        If Not File.Exists(configFilePath) Then
            _logger.LogError($"Config file not found at {configFilePath}")
            Return New List(Of String)()
        End If

        ' Only read lines not commented out with //
        'Return File.ReadAllLines(configFilePath).ToList()

        Dim lines As String() = File.ReadAllLines(configFilePath)
        Dim nonCommentedLines As New List(Of String)
        For Each line As String In lines
            If Not line.StartsWith("//") Then
                nonCommentedLines.Add(line)
            End If
        Next

        Return nonCommentedLines

    End Function


    ' Add the directory to the .tconfig file if it's not already in it
    Public Sub AddDirectoryToConfig(Optional directoryPath As String = Nothing) Implements IUtility.AddDirectoryToConfig
        If String.IsNullOrEmpty(directoryPath) Then
            directoryPath = Directory.GetCurrentDirectory()
            '_logger.LogError("{Method}: Directory path is null or empty", NameOf(AddDirectoryToConfig))
            'Return
        End If

        Dim configFilePath As String = GetConfigFilePath() 'Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tconfig")
        Dim directories As New List(Of String)

        If File.Exists(configFilePath) Then
            directories.AddRange(File.ReadAllLines(configFilePath))
        End If

        If Not directories.Contains(directoryPath) Then
            directories.Add(directoryPath)
            File.WriteAllLines(configFilePath, directories)
        End If
    End Sub

    Public Sub AddFilesToTransclude(directoryPath As String, Optional filename As String = "") Implements IUtility.AddFilesToTransclude
        Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
        Dim filesToTransclude As List(Of String) = GetFilesToTransclude(directoryPath)

        Dim existsFile As Boolean = File.Exists(transcludeFilePath)

        If Not String.IsNullOrEmpty(filename) AndAlso existsFile Then
            ' Add only the specific file if it's of interest
            If _fileSync.IsFileOfInterest(filename) AndAlso Not filesToTransclude.Contains(filename) Then
                filesToTransclude.Add(filename)
            End If
        Else
            ' Add all files of interest in the directory
            Dim filesInDirectory As String() = Directory.GetFiles(directoryPath)
            For Each filePath As String In filesInDirectory
                If _fileSync.IsFileOfInterest(filePath) AndAlso Not filesToTransclude.Contains(filePath) Then
                    filesToTransclude.Add(filePath)
                End If
            Next
        End If

        ' Write the updated list of files to the .transclude file
        File.WriteAllLines(transcludeFilePath, filesToTransclude)
    End Sub

    Public Function GetConfigFilePath() As String

        Dim filePath As String

        Dim pathFromConfig As String = _options.Value.AdminSettings.ConfigFilePath

        Dim configFilePath As String = path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tconfig")

        If Not String.IsNullOrEmpty(pathFromConfig) Then

            ' Use the path from the config file if it's not null or empty
            filePath = pathFromConfig
        Else

            ' Otherwise use the default path
            filePath = configFilePath
        End If

        Return filePath
    End Function

    Public Sub ManualSync(Optional syncDir As String = Nothing) Implements IUtility.ManualSync

        Dim monitorDirectories As New List(Of String)

        ' Check if a specific directory is provided and use that, otherwise use all directories in the config file
        If Not String.IsNullOrEmpty(syncDir) Then
            'monitorDirectories = monitorDirectories.Where(Function(dir) dir.Equals(syncDir, StringComparison.OrdinalIgnoreCase)).ToList()
            monitorDirectories.Add(syncDir)
        Else
            monitorDirectories = ReadMonitorDirectories()
        End If

        ' Filter out subdirectories that are already covered
        monitorDirectories = FilterRedundantDirectories(monitorDirectories)

        ' Filter out the application directory
        monitorDirectories = FilterAppDir(monitorDirectories)

        For Each monDir In monitorDirectories
            If Not String.IsNullOrEmpty(monDir) AndAlso Directory.Exists(monDir) Then

                Dim transcludeFiles As List(Of String) = GetFilesToTransclude(monDir)
                For Each file In transcludeFiles
                    _fileSync.SyncSourceToMarkdown(file)
                Next

            End If

        Next
    End Sub

    ' Filter out the application directory
    Public Function FilterAppDir(monitorDirectories As List(Of String)) As List(Of String) Implements IUtility.FilterAppDir

        Dim appDir As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

        Return monitorDirectories.Where(Function(dir) Not dir.Equals(appDir, StringComparison.OrdinalIgnoreCase)).ToList()

    End Function

    ' Filter out subdirectories that are already covered
    Public Function FilterRedundantDirectories(directories As List(Of String)) As List(Of String) Implements IUtility.FilterRedundantDirectories
        ' This list will hold the filtered directories
        Dim filteredDirectories As New List(Of String)

        For Each dir As String In directories
            ' Check if there's any directory in the list that is a parent of 'dir'
            Dim isSubdirectory As Boolean = directories.Any(Function(otherDir)
                                                                Return Not otherDir.Equals(dir, StringComparison.OrdinalIgnoreCase) AndAlso
                                                                   dir.StartsWith(otherDir, StringComparison.OrdinalIgnoreCase)
                                                            End Function)

            ' If 'dir' is not a subdirectory of any other directory in the list, add it to the filtered list
            If Not isSubdirectory Then
                filteredDirectories.Add(dir)
            End If
        Next

        Return filteredDirectories
    End Function


End Class
