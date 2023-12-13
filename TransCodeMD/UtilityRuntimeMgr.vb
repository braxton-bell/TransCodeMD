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

        ' ------------ Soure File Scenarios ------------
        ' 1. The user has specified a specific file to monitor (add only that file)
        ' 2. The user has specified a directory to monitor (add all files in the specified directory)
        ' 3. The user has not specified a file or directory to monitor (add all files in the current directory)

        If Not String.IsNullOrEmpty(_cliMgr.AddSourceFile) Then
            'If _cliMgr.AddSourceFile AndAlso Not IsNothing(_cliMgr.AddSourceFile) Then

            result = AddSourceFileToTransclude(_cliMgr.AddSourceFile)

        End If


        'If _cliMgr.AddSourceFilesFromDirectory And IsNothing(_cliMgr.SourceFilePath) Then
        If _cliMgr.AddSourceFilesFromDirectory And String.IsNullOrEmpty(_cliMgr.SourceFilePath) Then

            result = AddSourceFilesFromCurrentDirectoryToTransclude()

        End If

        If _cliMgr.AddSourceFilesFromDirectory And Not String.IsNullOrEmpty(_cliMgr.SourceFilePath) Then

            result = AddSourceFilesFromDirectoryToTransclude(_cliMgr.SourceFilePath)

        End If
        ' ------------ End Soure File Scenarios ------------


        ' ------------ Root Path Scenarios ------------
        If _cliMgr.AddRootPath AndAlso String.IsNullOrEmpty(_cliMgr.RootPath) Then

            result = AddCurrentDirectoryToConfig()

        End If

        If _cliMgr.AddRootPath AndAlso Not String.IsNullOrEmpty(_cliMgr.RootPath) Then

            result = AddNamedDirectoryToConfig(_cliMgr.RootPath)

        End If
        ' ------------ End Root Path Scenarios ------------


        ' ------------ Sync Scenarios ------------
        If _cliMgr.SyncFiles And String.IsNullOrEmpty(_cliMgr.SyncPath) Then

            result = SyncFilesFromConfig()

        End If

        If _cliMgr.SyncFiles And Not String.IsNullOrEmpty(_cliMgr.SyncPath) Then

            result = SyncFilesInNamedDirectory(_cliMgr.SyncPath)

        End If
        ' ------------ End Sync Scenarios ------------

        Return result
    End Function

    Private Function SyncFilesInNamedDirectory(syncPath As String) As OperationResult
        Throw New NotImplementedException()
    End Function

    Private Function SyncFilesFromConfig() As OperationResult
        Throw New NotImplementedException()
    End Function

    ''' <summary>
    ''' This method adds all files in a named directory to the .transclude file.
    ''' </summary>
    ''' <param name="sourceFilePath">The path to the directory containing the files to be added to the .transclude file.</param>
    ''' <returns></returns>
    Private Function AddSourceFilesFromDirectoryToTransclude(sourceFilePath As String) As OperationResult
        Dim result = OperationResult.Ok

        Try
            _utility.AddAllFilesInDirectoryToTransclude(sourceFilePath)
        Catch ex As Exception
            _logger.LogWarning(ex, "{Method}: Error adding source files to transclude: {sourceFilePath}", NameOf(AddSourceFilesFromDirectoryToTransclude), sourceFilePath)
            result = OperationResult.Fail(ex.Message)
        End Try


        Return result
    End Function

    ''' <summary>
    ''' This method adds all files in the current directory to the .transclude file.
    ''' </summary>
    ''' <returns></returns>
    Private Function AddSourceFilesFromCurrentDirectoryToTransclude() As OperationResult
        Dim result = OperationResult.Ok

        Dim currentDirectory = Directory.GetCurrentDirectory()

        Try
            _utility.AddAllFilesInDirectoryToTransclude(currentDirectory)
        Catch ex As Exception
            _logger.LogWarning(ex, "{Method}: Error adding source files to transclude: {currentDirectory}", NameOf(AddSourceFilesFromCurrentDirectoryToTransclude), currentDirectory)
            result = OperationResult.Fail(ex.Message)
        End Try

        Return result
    End Function

    ''' <summary>
    ''' This method adds a specific file to the .transclude file.
    ''' </summary>
    ''' <param name="addSourceFile">The file to be added to the .transclude file.</param>
    ''' <returns></returns>
    Private Function AddSourceFileToTransclude(addSourceFile As String) As OperationResult
        Dim result = OperationResult.Ok

        Try
            _utility.AddSpecificFileToTransclude(addSourceFile)
        Catch ex As Exception
            _logger.LogWarning(ex, "{Method}: Error adding source file to transclude: {addSourceFile}", NameOf(AddSourceFileToTransclude), addSourceFile)
            result = OperationResult.Fail(ex.Message)
        End Try

        Return result

    End Function

    ''' <summary>
    ''' This method adds the current directory to the config.
    ''' </summary>
    ''' <returns></returns>
    Private Function AddCurrentDirectoryToConfig() As IOperationResult
        Dim result = OperationResult.Ok

        Try
            Dim currentDirectory = Directory.GetCurrentDirectory()

            If _ui.ConfirmAddDirectoryToConfig(currentDirectory) Then
                _utility.WriteTConfig()
                _logger.LogInformation("{Method}: {CurrentDirectory} has been added to the config.", NameOf(AddCurrentDirectoryToConfig), currentDirectory)
            Else
                _logger.LogInformation("{Method}: {CurrentDirectory} has NOT been added to the config.", NameOf(AddCurrentDirectoryToConfig), currentDirectory)
            End If
        Catch ex As Exception
            _logger.LogWarning(ex, "{Method}: Error adding current directory to config.", NameOf(AddCurrentDirectoryToConfig))
            result = OperationResult.Fail(ex.Message)
        End Try

        Return result
    End Function

    ''' <summary>
    ''' This method adds a named directory to the config.
    ''' </summary>
    ''' <param name="dirPath">Path to the directory to be added to the config.</param>
    ''' <returns></returns>
    Private Function AddNamedDirectoryToConfig(dirPath As String) As IOperationResult
        Dim result = OperationResult.Ok

        Try
            If _ui.ConfirmAddDirectoryToConfig(dirPath) Then
                _utility.WriteTConfig(dirPath)
                _logger.LogInformation("{Method}: {dirPath} has been added to the config.", NameOf(AddNamedDirectoryToConfig), dirPath)
            Else
                _logger.LogInformation("{Method}: {dirPath} has NOT been added to the config.", NameOf(AddNamedDirectoryToConfig), dirPath)
            End If
        Catch ex As Exception
            _logger.LogWarning(ex, "{Method}: Error adding directory to config: {dirPath}", NameOf(AddNamedDirectoryToConfig), dirPath)
            result = OperationResult.Fail(ex.Message)
        End Try

        Return result
    End Function
End Class
