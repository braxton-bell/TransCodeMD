Imports System.IO
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUtility
    Sub AddFilesToTransclude(directoryPath As String)
    Function GetFilesToTransclude(directoryPath As String) As List(Of String)
    Sub CreateTranscludeFile(directoryPath As String)
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
    Public Sub AddFilesToTransclude(directoryPath As String) Implements IUtility.AddFilesToTransclude
        Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
        Dim filesToTransclude As List(Of String) = GetFilesToTransclude(directoryPath)

        ' Get all files in the directory
        Dim filesInDirectory As String() = Directory.GetFiles(directoryPath)

        ' Add all files that aren't already in the .transclude file and are "of interest"
        For Each filePath As String In filesInDirectory
            If _fileSync.IsFileOfInterest(filePath) AndAlso Not filesToTransclude.Contains(filePath) Then
                filesToTransclude.Add(filePath)
            End If
        Next

        ' Write the updated list of files to the .transclude file
        File.WriteAllLines(transcludeFilePath, filesToTransclude)
    End Sub

End Class
