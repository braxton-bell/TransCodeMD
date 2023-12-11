Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IFileSync
    Sub SyncMarkdownToSource(markdownFilePath As String)
    Sub SyncSourceToMarkdown(sourceFilePath As String)
    Function ConvertSourceToMarkdown(sourceContent As String, langId As String) As String
    Function ExtractSourceFromMarkdown(markdownContent As String) As String
    Function GetFileExtFromLangId(langId As String) As String
    Function GetLangIdFromFileExt(sourceFilePath As String) As String
    Function IsFileOfInterest(filePath As String) As Boolean
    Function ReadLanguageMappings() As Dictionary(Of String, String)
    Sub MarkFileAsStale(oldSourceFilePath As String)
End Interface


Public Class FileSync
    Implements IFileSync

    Private ReadOnly _userInteraction As IUserInteraction
    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of FileSync)

    Public Sub New(userInteraction As IUserInteraction, options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of FileSync))
        _userInteraction = userInteraction
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    Private Function IsSourceNewerThanMarkdown(sourceFilePath As String, markdownFilePath As String) As Boolean
        Dim sourceFileInfo As New FileInfo(sourceFilePath)
        Dim markdownFileInfo As New FileInfo(markdownFilePath)

        ' Check if Markdown file exists
        If Not markdownFileInfo.Exists Then
            Return True ' Markdown doesn't exist, source is considered newer
        End If

        Return sourceFileInfo.LastWriteTimeUtc > markdownFileInfo.LastWriteTimeUtc
    End Function

    Public Function IsFileOfInterest(filePath As String) As Boolean Implements IFileSync.IsFileOfInterest
        Dim languages As Dictionary(Of String, String) = ReadLanguageMappings()
        'Dim extensionsOfInterest As String() = {".py", ".cs", ".vb", ".ps1", ".md"}

        ' Get Extensions from Language Mappings, 2nd column
        Dim extensionsOfInterest As String() = languages.Values.ToArray()

        Dim fileExtension As String = Path.GetExtension(filePath)

        'Return extensionsOfInterest.Contains(fileExtension.ToLower())

        ' Return true if the file extension is in the list of interest and it's not a .md file
        Return extensionsOfInterest.Contains(fileExtension) AndAlso fileExtension.ToLower <> ".md"

    End Function

    ' Read the configuration from appsettings.json
    Public Function ReadLanguageMappings() As Dictionary(Of String, String) Implements IFileSync.ReadLanguageMappings
        Dim mappings As Dictionary(Of String, String) = _options.Value.LanguageMappings

        Return mappings
    End Function

    Public Function GetLangIdFromFileExt(sourceFilePath As String) As String Implements IFileSync.GetLangIdFromFileExt
        Dim mappings As Dictionary(Of String, String) = ReadLanguageMappings()
        Dim fileExt As String = Path.GetExtension(sourceFilePath).ToLower()

        ' Find the corresponding language identifier
        Dim langId As String = mappings.FirstOrDefault(Function(m) m.Value.Equals(fileExt, StringComparison.OrdinalIgnoreCase)).Key
        If String.IsNullOrEmpty(langId) Then
            ' Handle unsupported file extensions if needed
            Return String.Empty
        End If

        Return langId
    End Function

    Public Function GetFileExtFromLangId(langId As String) As String Implements IFileSync.GetFileExtFromLangId
        Dim mappings As Dictionary(Of String, String) = ReadLanguageMappings()
        Dim normalizedLangId As String = langId.ToLower()

        ' Find the corresponding file extension
        If mappings.ContainsKey(normalizedLangId) Then
            Return mappings(normalizedLangId)
        Else
            ' Handle unsupported language identifiers if needed
            Return ".txt"
        End If
    End Function

    ' Sync changes from source to Markdown
    Public Sub SyncSourceToMarkdown(sourceFilePath As String) Implements IFileSync.SyncSourceToMarkdown
        ' Determine the path of the corresponding Markdown file
        Dim markdownFilePath As String = sourceFilePath & ".md"

        ' Check if Markdown file exists and whether it was updated by the app
        Dim markdownExists = File.Exists(markdownFilePath)
        Dim isUpdatedByApp = IsMarkdownUpdatedByApp(markdownFilePath)

        ' Check if the source file is newer or if user confirms sync when Markdown is newer
        If Not markdownExists OrElse isUpdatedByApp OrElse IsSourceNewerThanMarkdown(sourceFilePath, markdownFilePath) OrElse
           _userInteraction.ConfirmSyncForNewerMarkdown() Then

            Dim langId As String = GetLangIdFromFileExt(sourceFilePath)
            Dim markdownContent As String = ConvertSourceToMarkdown(sourceFilePath, langId)

            ' Write the updated content to the Markdown file
            File.WriteAllText(markdownFilePath, markdownContent)
            'Console.WriteLine("Synchronization completed.")
            _logger.LogInformation("{Method}: Synchronization completed.", NameOf(SyncSourceToMarkdown))

        Else
            'Console.WriteLine("Synchronization skipped.")
            _logger.LogInformation("{Method}: Synchronization skipped.", NameOf(SyncSourceToMarkdown))
        End If

    End Sub

    Public Function ConvertSourceToMarkdown(sourceContent As String, langId As String) As String Implements IFileSync.ConvertSourceToMarkdown

        ' Read the source file and wrap its contents in Markdown code block syntax
        sourceContent = File.ReadAllText(sourceContent)
        Dim markdownContent As String

        markdownContent = $"```{langId}{Environment.NewLine}{sourceContent}{Environment.NewLine}```"

        ' Add a marker to the Markdown file to indicate that it was updated by TransCodeMD
        markdownContent = MarkMarkdownFile(markdownContent)

        Return markdownContent
    End Function

    ' Add a marker to the Markdown file to indicate that it was updated by TransCodeMD
    Private Function MarkMarkdownFile(markdownContent As String) As String
        'Dim marker As String = Environment.NewLine & "<!-- Updated by TransCodeMD -->" & Environment.NewLine
        'File.AppendAllText(markdownFilePath, marker)

        'Dim timestamp As String = DateTime.Now.ToString("yyyyMMddHHmmss")
        Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        Dim marker As String = Environment.NewLine & $"<!-- Updated by TransCodeMD [{timestamp}] -->" & Environment.NewLine

        ' Append the marker to the Markdown content
        Return markdownContent & marker

    End Function

    Private Function IsMarkdownUpdatedByApp(markdownFilePath As String) As Boolean
        Dim marker As String = "<!-- Updated by TransCodeMD -->"

        'If File.ReadAllText(markdownFilePath).Contains(marker) Then
        '    Return True
        'End If

        'Return False

        ' Check if the Markdown file exists
        If Not File.Exists(markdownFilePath) Then
            Return False ' The file does not exist, so it wasn't updated by the app
        End If

        Dim markdownContent As String = File.ReadAllText(markdownFilePath)
        'Dim markerPattern As String = "<!-- Updated by TransCodeMD \[\d{14}\] -->"
        Dim markerPattern As String = "<!-- Updated by TransCodeMD \[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] -->"

        ' Check if the Markdown file contains the marker
        'Return File.ReadAllText(markdownFilePath).Contains(marker)

        Return Regex.IsMatch(markdownContent, markerPattern)

    End Function

    Private Function IsMarkedAsStale(oldMarkdownFilePath As String) As Boolean
        ' Sample stale notice: >TransCodeMD Notice: Source Script Renamed. The current reference is stale.- [2023-12-11 12:55:20]
        Dim staleNoticePattern As String = "^>TransCodeMD Notice: Source Script Renamed. The current reference is stale.- \[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\]$"
        ' Check if the Markdown file exists
        If Not File.Exists(oldMarkdownFilePath) Then
            Return False ' The file does not exist, so it wasn't updated by the app
        End If

        Dim markdownContent As String = File.ReadAllText(oldMarkdownFilePath)

        ' Check if the Markdown file contains the stale notice
        Return Regex.IsMatch(markdownContent, staleNoticePattern, RegexOptions.Multiline)
    End Function

    Public Sub SyncMarkdownToSource(markdownFilePath As String) Implements IFileSync.SyncMarkdownToSource
        ' Determine the path of the corresponding source file
        Dim sourceFilePath As String = markdownFilePath.Substring(0, markdownFilePath.Length - 3)

        ' Read the Markdown file
        Dim markdownContent As String = File.ReadAllText(markdownFilePath)

        ' Extract the source code from the Markdown content
        Dim sourceContent As String = ExtractSourceFromMarkdown(markdownContent)

        ' Write the updated content to the source file
        File.WriteAllText(sourceFilePath, sourceContent)
    End Sub

    ' This function extracts the source code from the Markdown file content
    Public Function ExtractSourceFromMarkdown(markdownContent As String) As String Implements IFileSync.ExtractSourceFromMarkdown
        ' Assuming the source code is wrapped in code block syntax (```),
        ' we need to find the start and end of the code block
        Dim codeBlockStart As String = "```"
        Dim startIndex As Integer = markdownContent.IndexOf(codeBlockStart)

        If startIndex = -1 Then
            ' Code block start not found, return empty string or handle as needed
            Return String.Empty
        End If

        ' Find the end of the line where the code block starts
        ' This is to skip the language identifier (like ```vb)
        Dim endOfStartLineIndex As Integer = markdownContent.IndexOf(Environment.NewLine, startIndex)
        If endOfStartLineIndex = -1 Then
            ' New line not found after the code block start, handle as needed
            Return String.Empty
        End If

        ' Adjust start index to be after the newline character
        startIndex = endOfStartLineIndex + Environment.NewLine.Length

        ' Find the end of the code block
        Dim endIndex As Integer = markdownContent.LastIndexOf(codeBlockStart)
        If endIndex = -1 OrElse endIndex <= startIndex Then
            ' Invalid code block, handle as needed
            Return String.Empty
        End If

        ' Extract the source code from within the code block
        Return markdownContent.Substring(startIndex, endIndex - startIndex).Trim()
    End Function

    ' This function marks a file as stale by adding a notice to the beginning of the Markdown content
    Public Sub MarkFileAsStale(oldSourceFilePath As String) Implements IFileSync.MarkFileAsStale
        ' Determine the path of the corresponding old Markdown file
        Dim oldMarkdownFilePath As String = oldSourceFilePath & ".md"

        ' Check if Markdown file exists and whether it was updated by the app
        Dim markdownExists = File.Exists(oldMarkdownFilePath)
        Dim markedAsStale = IsMarkedAsStale(oldMarkdownFilePath)

        ' Check if the Markdown file exists
        If Not markdownExists Then
            _logger.LogWarning("{Method}: Markdown file for stale source not found: {OldMarkdownFilePath}", NameOf(MarkFileAsStale), oldMarkdownFilePath)
            Return
        End If

        ' Check if the Markdown file was updated by the app
        If markedAsStale Then
            ' File was already marked as stale, so no need to mark it again
            Return
        End If

        ' Read the existing Markdown content
        Dim markdownContent As String = File.ReadAllText(oldMarkdownFilePath)

        Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

        ' Define the stale notice
        Dim staleNotice As String = $">TransCodeMD Notice: Source Script Renamed. The current reference is stale.- [{timestamp}]" & Environment.NewLine

        ' Insert the stale notice at the beginning of the Markdown content
        markdownContent = staleNotice & markdownContent

        'markdownContent = MarkMarkdownFile(markdownContent)

        ' Write the updated content back to the Markdown file
        File.WriteAllText(oldMarkdownFilePath, markdownContent)

        _logger.LogInformation($"Marked file as stale: {oldMarkdownFilePath}")
    End Sub


End Class
