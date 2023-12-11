Imports System.IO
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

    Private ReadOnly _fileSync As IFileSync

    ' Directory to monitor - this should be replaced with your actual directory path
    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of FileMonitor)

    Private _monitorDirectory As String = "C:\source\repos\TransCodeMD\demo"

    Public Sub New(fileSync As IFileSync, options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of FileMonitor))
        _fileSync = fileSync
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    Public Async Function RunAsync() As Task(Of IOperationResult) Implements IMonitor.RunAsync
        Dim result = OperationResult.Ok

        'Call Monitor()

        Call _fileSync.SyncSourceToMarkdown("C:\source\repos\TransCodeMD\demo\playbook.yaml")

        'Call SyncMarkdownToSource("C:\source\repos\TransCodeMD\demo\script.vb.md")

        'Call AddFilesToTransclude("C:\source\repos\TransCodeMD\demo")

        'Dim files = GetFilesToTransclude("C:\source\repos\TransCodeMD\demo")
        'For Each file In files
        '    System.Console.WriteLine(file)
        'Next

        Await Task.Delay(1000)

        Return result

    End Function

    Private Sub Monitor()
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
            'Console.WriteLine("Press 'q' to quit the sample.")
            _logger.LogInformation("Press 'q' to quit the sample.")


            While Char.ToLowerInvariant(System.Console.ReadKey().KeyChar) <> "q"c
                System.Threading.Thread.Sleep(1000)
            End While

        End Using
    End Sub


    ' Define the event handlers.

    Private Sub OnChanged(sender As Object, e As FileSystemEventArgs)
        If _fileSync.IsFileOfInterest(e.FullPath) Then
            ' Implement your sync logic here
            System.Console.WriteLine($"File of interest changed: {e.FullPath} {e.ChangeType}")
        End If
    End Sub

    Private Sub OnRenamed(sender As Object, e As RenamedEventArgs)
        ' Specify what is done when a file is renamed.
        _logger.LogInformation("{Method}: File: {OldFullPath} renamed to {FullPath}", NameOf(OnRenamed), e.OldFullPath, e.FullPath)
    End Sub

    'Private Function IsFileOfInterest(filePath As String) As Boolean

    '    Dim languages As Dictionary(Of String, String) = ReadLanguageMappings()
    '    'Dim extensionsOfInterest As String() = {".py", ".cs", ".vb", ".ps1", ".md"}

    '    ' Get Extensions from Language Mappings, 2nd column
    '    Dim extensionsOfInterest As String() = languages.Values.ToArray()

    '    Dim fileExtension As String = Path.GetExtension(filePath)

    '    Return extensionsOfInterest.Contains(fileExtension.ToLower())

    'End Function


    ' Create a .transclude file in the directory if it doesn't already exist
    'Private Sub CreateTranscludeFile(directoryPath As String)
    '    Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")

    '    ' Check if the file already exists
    '    If Not File.Exists(transcludeFilePath) Then
    '        ' Create an empty .transclude file
    '        File.WriteAllText(transcludeFilePath, String.Empty)
    '        'Console.WriteLine($"Created new .transclude file at: {transcludeFilePath}")
    '        _logger.LogInformation("{Method}: Created new .transclude file at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
    '    Else
    '        'Console.WriteLine($"A .transclude file already exists at: {transcludeFilePath}")
    '        _logger.LogInformation("{Method}: A .transclude file already exists at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
    '    End If
    'End Sub


    ' Read the .transclude file and return a list of all files that should be transcluded
    'Private Function GetFilesToTransclude(directoryPath As String) As List(Of String)
    '    Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
    '    If Not File.Exists(transcludeFilePath) Then
    '        Return New List(Of String)()
    '    End If

    '    ' Read all lines from the .transclude file
    '    Dim lines As String() = File.ReadAllLines(transcludeFilePath)
    '    Return lines.ToList()
    'End Function



    ' Add all files in the directory to the .transclude file that aren't already in it and are "of interest"
    'Private Sub AddFilesToTransclude(directoryPath As String)
    '    Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")
    '    Dim filesToTransclude As List(Of String) = GetFilesToTransclude(directoryPath)

    '    ' Get all files in the directory
    '    Dim filesInDirectory As String() = Directory.GetFiles(directoryPath)

    '    ' Add all files that aren't already in the .transclude file and are "of interest"
    '    For Each filePath As String In filesInDirectory
    '        If IsFileOfInterest(filePath) AndAlso Not filesToTransclude.Contains(filePath) Then
    '            filesToTransclude.Add(filePath)
    '        End If
    '    Next

    '    ' Write the updated list of files to the .transclude file
    '    File.WriteAllLines(transcludeFilePath, filesToTransclude)
    'End Sub


    ' Sync changes from source to Markdown
    'Private Sub SyncSourceToMarkdown(sourceFilePath As String)
    '    ' Determine the path of the corresponding Markdown file
    '    Dim markdownFilePath As String = sourceFilePath & ".md"

    '    Dim langId As String

    '    langId = GetLangIdFromFileExt(sourceFilePath)

    '    Dim markdownContent As String = ConvertSourceToMarkdown(sourceFilePath, langId)

    '    ' Write the updated content to the Markdown file
    '    File.WriteAllText(markdownFilePath, markdownContent)
    'End Sub

    'Private Function ConvertSourceToMarkdown(sourceContent As String, langId As String) As String

    '    ' Read the source file and wrap its contents in Markdown code block syntax
    '    sourceContent = File.ReadAllText(sourceContent)
    '    Dim markdownContent As String

    '    markdownContent = $"```{langId}{Environment.NewLine}{sourceContent}{Environment.NewLine}```"

    '    Return markdownContent
    'End Function

    ' Read the configuration from appsettings.json
    'Private Function ReadLanguageMappings() As Dictionary(Of String, String)

    '    Dim mappings As Dictionary(Of String, String) = _options.Value.LanguageMappings

    '    Return mappings
    'End Function

    'Private Function GetLangIdFromFileExt(sourceFilePath As String) As String
    '    Dim mappings As Dictionary(Of String, String) = ReadLanguageMappings()
    '    Dim fileExt As String = Path.GetExtension(sourceFilePath).ToLower()

    '    ' Find the corresponding language identifier
    '    Dim langId As String = mappings.FirstOrDefault(Function(m) m.Value.Equals(fileExt, StringComparison.OrdinalIgnoreCase)).Key
    '    If String.IsNullOrEmpty(langId) Then
    '        ' Handle unsupported file extensions if needed
    '        Return String.Empty
    '    End If

    '    Return langId
    'End Function

    'Private Function GetFileExtFromLangId(langId As String) As String
    '    Dim mappings As Dictionary(Of String, String) = ReadLanguageMappings()
    '    Dim normalizedLangId As String = langId.ToLower()

    '    ' Find the corresponding file extension
    '    If mappings.ContainsKey(normalizedLangId) Then
    '        Return mappings(normalizedLangId)
    '    Else
    '        ' Handle unsupported language identifiers if needed
    '        Return ".txt"
    '    End If
    'End Function

    'Private Sub SyncMarkdownToSource(markdownFilePath As String)
    '    ' Determine the path of the corresponding source file
    '    Dim sourceFilePath As String = markdownFilePath.Substring(0, markdownFilePath.Length - 3)

    '    ' Read the Markdown file
    '    Dim markdownContent As String = File.ReadAllText(markdownFilePath)

    '    ' Extract the source code from the Markdown content
    '    Dim sourceContent As String = ExtractSourceFromMarkdown(markdownContent)

    '    ' Write the updated content to the source file
    '    File.WriteAllText(sourceFilePath, sourceContent)
    'End Sub

    '' This function extracts the source code from the Markdown file content
    'Private Function ExtractSourceFromMarkdown(markdownContent As String) As String
    '    ' Assuming the source code is wrapped in code block syntax (```),
    '    ' we need to find the start and end of the code block
    '    Dim codeBlockStart As String = "```"
    '    Dim startIndex As Integer = markdownContent.IndexOf(codeBlockStart)

    '    If startIndex = -1 Then
    '        ' Code block start not found, return empty string or handle as needed
    '        Return String.Empty
    '    End If

    '    ' Find the end of the line where the code block starts
    '    ' This is to skip the language identifier (like ```vb)
    '    Dim endOfStartLineIndex As Integer = markdownContent.IndexOf(Environment.NewLine, startIndex)
    '    If endOfStartLineIndex = -1 Then
    '        ' New line not found after the code block start, handle as needed
    '        Return String.Empty
    '    End If

    '    ' Adjust start index to be after the newline character
    '    startIndex = endOfStartLineIndex + Environment.NewLine.Length

    '    ' Find the end of the code block
    '    Dim endIndex As Integer = markdownContent.LastIndexOf(codeBlockStart)
    '    If endIndex = -1 OrElse endIndex <= startIndex Then
    '        ' Invalid code block, handle as needed
    '        Return String.Empty
    '    End If

    '    ' Extract the source code from within the code block
    '    Return markdownContent.Substring(startIndex, endIndex - startIndex).Trim()
    'End Function

End Class
