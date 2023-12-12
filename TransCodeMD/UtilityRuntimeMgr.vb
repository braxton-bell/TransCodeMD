Imports System.IO
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUtilityRuntimeMgr
    Function Run() As IOperationResult
End Interface

Public Class UtilityRuntimeMgr
    Implements IUtilityRuntimeMgr

    Private ReadOnly _ui As IUserInteraction
    Private ReadOnly _cliMgr As ICliManager
    Private ReadOnly _utility As IUtility
    Private ReadOnly _fileSync As IFileSync
    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of UtilityRuntimeMgr)

    Public Sub New(ui As IUserInteraction,
                   cliMgr As ICliManager,
                   utility As IUtility,
                   fileSync As IFileSync,
                   options As IOptions(Of ApplicationConfig),
                   propMgr As ILogPropertyMgr,
                   logger As ILogger(Of UtilityRuntimeMgr))
        _ui = ui
        _cliMgr = cliMgr
        _utility = utility
        _fileSync = fileSync
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    Public Function Run() As IOperationResult Implements IUtilityRuntimeMgr.Run
        Dim result = OperationResult.Ok

        If _cliMgr.MonitorRootDir AndAlso String.IsNullOrEmpty(_cliMgr.DirectoryPath) Then

            result = AddThisDirectoryToConfig()

        End If

        If _cliMgr.MonitorRootDir AndAlso Not String.IsNullOrEmpty(_cliMgr.DirectoryPath) Then

            result = AddThatDirectoryToConfig(_cliMgr.DirectoryPath)

        End If

        If _cliMgr.MonitorSourceFile Then

            result = AddSourceFilesToMonitor(_cliMgr.MonitorSourceFile)

        End If

        Return result
    End Function

    Private Function AddThisDirectoryToConfig() As IOperationResult
        Dim result = OperationResult.Ok

        Dim currentDirectory = Directory.GetCurrentDirectory()

        If _ui.ConfirmAddDirectoryToConfig(currentDirectory) Then
            _utility.AddDirectoryToConfig()
            _logger.LogInformation("{Method}: {CurrentDirectory} has been added to the config.", NameOf(AddThisDirectoryToConfig), currentDirectory)
        Else
            _logger.LogInformation("{Method}: {CurrentDirectory} has NOT been added to the config.", NameOf(AddThisDirectoryToConfig), currentDirectory)
        End If

        Return result
    End Function

    Private Function AddThatDirectoryToConfig(dirPath As String) As IOperationResult
        Dim result = OperationResult.Ok

        If _ui.ConfirmAddDirectoryToConfig(dirPath) Then
            _utility.AddDirectoryToConfig(dirPath)
            _logger.LogInformation("{Method}: {dirPath} has been added to the config.", NameOf(AddThatDirectoryToConfig), dirPath)
        Else
            _logger.LogInformation("{Method}: {dirPath} has NOT been added to the config.", NameOf(AddThatDirectoryToConfig), dirPath)
        End If

        Return result
    End Function

    'Private Function AddSourceFilesToMonitor(sourceFilePath As String) As IOperationResult
    '    Dim result = OperationResult.Ok

    '    ' Scenarios:
    '    ' 1. The user has specified a specific file to monitor (add only that file)
    '    ' 2. The user has specified a directory to monitor (add all files in the specified directory)
    '    ' 3. The user has not specified a file or directory to monitor (add all files in the current directory)

    '    Return result
    'End Function

    Private Function AddSourceFilesToMonitor(Optional sourceFilePath As String = "") As IOperationResult
        Dim result = OperationResult.Ok

        Dim directoryPath As String
        Dim specificFile As String = ""

        ' Check if a specific file or directory is provided
        If Not String.IsNullOrEmpty(sourceFilePath) Then
            If File.Exists(sourceFilePath) Then
                ' Scenario 1: User specified a specific file
                directoryPath = Path.GetDirectoryName(sourceFilePath)
                specificFile = Path.GetFileName(sourceFilePath)
            ElseIf Directory.Exists(sourceFilePath) Then
                ' Scenario 2: User specified a directory
                directoryPath = sourceFilePath
            Else
                ' Invalid path provided
                Return OperationResult.Fail($"Invalid path: {sourceFilePath}")
            End If
        Else
            ' Scenario 3: No specific file or directory provided; use current directory
            directoryPath = Directory.GetCurrentDirectory()
        End If

        ' Now, add files to transclude based on the determined path
        Try
            If Not String.IsNullOrEmpty(specificFile) Then
                ' Add only the specific file
                If _fileSync.IsFileOfInterest(specificFile) Then
                    _utility.AddFilesToTransclude(directoryPath, sourceFilePath)
                End If
            Else
                ' Add all files of interest in the directory
                _utility.AddFilesToTransclude(directoryPath)
            End If
        Catch ex As Exception
            '_logger.LogError(ex, "Error in adding source files to monitor.")
            _logger.LogError(ex, "{Method}: Error in adding source files to monitor.", NameOf(AddSourceFilesToMonitor))
            result = OperationResult.Fail(ex.Message)
        End Try

        Return result
    End Function


End Class
