Imports System.IO
Imports Microsoft.AspNetCore.Mvc.TagHelpers
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUtility
    'Sub AddFilesToTransclude(directoryPath As String, Optional specificFilePath As String = "")
    Function ReadTranscludeFile(directoryPath As String) As List(Of String)
    Sub CreateTranscludeFile(directoryPath As String)
    Function ReadTConfig() As List(Of String)
    Sub WriteTConfig(Optional directoryPath As String = Nothing)
    Function FilterInstallDirFromList(monitorDirectories As List(Of String)) As List(Of String)
    Function FilterRedundantDirectoriesFromList(directories As List(Of String)) As List(Of String)
    Sub AdhocTranscode(Optional directory As String = Nothing)
    Sub AddSpecificFileToTransclude(filePath As String)
    Sub AddAllFilesInDirectoryToTransclude(directoryPath As String)
    'Function FilterInstallDir(directoryPath As String) As Boolean
    Function IsInstallDir(directoryPath As String) As Boolean
    Function ExistsFileInTransclude(dirPath As String) As Boolean
    Function GetTranscludeFilePath(sourceFilePath As String) As String
    Sub WriteTranscludeFile(sourceFilePath As String)
    Sub ListAllRefInLocalTranscodeFile()
    Sub ListAllRefInConfigFile()
    Function ExistsTranscludeFileAtPath(filePath As String) As Boolean
    Function GetProperDirectoryPath(targetPath As String) As String
    'Function ExistsTranscludeFileAtPath(filePath As String) As Boolean
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

    ''' <summary>
    ''' This method creates a .transclude file in the directory if it doesn't already exist.
    ''' </summary>
    ''' <param name="directoryPath">Full path of the directory to create the .transclude file in.</param>
    ''' <remarks>
    ''' This method will check if the `.transclude` file exists in the directory and create it if it doesn't.
    ''' </remarks>
    Public Sub CreateTranscludeFile(directoryPath As String) Implements IUtility.CreateTranscludeFile

        ' Verify that the directory exists
        If Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException($"Directory does not exist: {directoryPath}")
        End If


        If IsInstallDir(directoryPath) Then
            Throw New Exception("Cannot create .transclude file in the application directory.")
        End If

        Dim transcludeFilePath As String = Path.Combine(directoryPath, ".transclude")

        ' Check if the file already exists
        If Not File.Exists(transcludeFilePath) Then
            ' Create an empty .transclude file
            File.WriteAllText(transcludeFilePath, String.Empty)

            _logger.LogDebug("{Method}: Created new .transclude file at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
        Else

            _logger.LogDebug("{Method}: A .transclude file already exists at: {transcludeFilePath}", NameOf(CreateTranscludeFile), transcludeFilePath)
        End If
    End Sub

    ''' <summary>
    ''' This method reads the .transclude file in the directory and returns a list of all files that should be transcluded.
    ''' </summary>
    ''' <param name="targetPath">Full path of the directory containing the .transclude file.</param>
    ''' <remarks>
    ''' </remarks>
    ''' <returns></returns>
    Public Function ReadTranscludeFile(targetPath As String) As List(Of String) Implements IUtility.ReadTranscludeFile

        ' Verify that the directory exists
        If Not Directory.Exists(GetProperDirectoryPath(targetPath)) Then
            Throw New DirectoryNotFoundException($"Directory does not exist: {targetPath}")
        End If

        Dim transcludeFilePath As String = Path.Combine(targetPath, ".transclude")

        ' Catch empty transclude file
        If Not File.Exists(transcludeFilePath) Then
            Throw New FileNotFoundException($"File does not exist: {transcludeFilePath}")
        End If

        ' Create the .transclude file if it doesn't already exist
        'CreateTranscludeFile(directoryPath) ' DO NOT CREATE THE FILE HERE. Creating a .transclude file should be explicit.

        ' Read all lines from the .transclude file
        Dim lines As String() = File.ReadAllLines(transcludeFilePath)

        Return lines.ToList()

    End Function

    ''' <summary>
    ''' This method adds the source file (full path) to the .transclude file if it's not already in it.
    ''' </summary>
    ''' <remarks>
    ''' This method will check if the `.transclude` file exists in the directory and create it if it doesn't.
    ''' It will then read all lines from the `.transclude` file and add the source file to it if it's not already in it.
    ''' </remarks>
    ''' <param name="sourceFilePath">Full path of source file to be added to the `.transclude` file.</param>
    Public Sub WriteTranscludeFile(sourceFilePath As String) Implements IUtility.WriteTranscludeFile

        Dim directoryPath As String

        ' Verify that the source file exists
        If File.Exists(sourceFilePath) Then
            directoryPath = GetProperDirectoryPath(sourceFilePath)
        Else
            _logger.LogError("{Method}: Source file does not exist: {SourceFilePath}", NameOf(WriteTranscludeFile), sourceFilePath)
            Return
        End If

        ' Create the .transclude file if it doesn't already exist
        CreateTranscludeFile(directoryPath)

        ' Read all lines from the .transclude file
        Dim existingFilesInTransclude As List(Of String) = ReadTranscludeFile(directoryPath)

        ' Add the source file to the .transclude file if it's not already in it
        If Not existingFilesInTransclude.Contains(sourceFilePath) Then
            existingFilesInTransclude.Add(sourceFilePath)
            File.WriteAllLines(Path.Combine(directoryPath, ".transclude"), existingFilesInTransclude)
        End If

    End Sub

    ''' <summary>
    ''' This method checks if the source file exists in the .transclude file.
    ''' </summary>
    ''' <param name="filePath">Sore file or directory path.</param>
    ''' <remarks>Method will also return false if the .transclude file doesn't exist.</remarks>
    ''' <returns></returns>
    Public Function ExistsFileInTransclude(filePath As String) As Boolean Implements IUtility.ExistsFileInTransclude

        Dim exitsInTransclude As Boolean = False

        ' Verify that the source file exists
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"File does not exist: {filePath}")
        End If

        Dim existingFilesInTransclude As List(Of String)

        Try
            ' Read all lines from the .transclude file
            existingFilesInTransclude = ReadTranscludeFile(GetProperDirectoryPath(filePath))

        Catch ex As FileNotFoundException
            ' If the .transclude file doesn't exist, return false
            ' We dont want an exception here because we're just checking if the file exists.
            ' If it doesn't exist, then we know the source file reference can't exist in it.

            Return exitsInTransclude

        Catch ex As Exception
            ' Rethrow any other exceptions
            Throw
        End Try

        ' Check if the  source file reference exists in the .transclude file
        exitsInTransclude = existingFilesInTransclude.Contains(filePath)

        Return exitsInTransclude

    End Function

    ''' <summary>
    ''' This method returns the full path to the .transclude file in the directory.
    ''' </summary>
    ''' <param name="sourcePath">Soure file or directory path.</param>
    ''' <remarks>
    ''' This method will throw an exception if the directory doesn't exist or the .transclude file doesn't exist.
    ''' </remarks>
    ''' <returns></returns>
    Public Function GetTranscludeFilePath(sourcePath As String) As String Implements IUtility.GetTranscludeFilePath

        sourcePath = GetProperDirectoryPath(sourcePath)

        Dim transcludeFilePath As String = String.Empty

        ' Verify that the source path
        If Directory.Exists(sourcePath) Then
            transcludeFilePath = Path.Combine(sourcePath, ".transclude")
        Else
            Throw New DirectoryNotFoundException($"Directory does not exist: {sourcePath}")
        End If

        ' Check if .transclude file exists
        If Not File.Exists(transcludeFilePath) Then
            Throw New FileNotFoundException($"File does not exist: {transcludeFilePath}")
        End If

        Return transcludeFilePath

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

    Public Function ReadTConfig() As List(Of String) Implements IUtility.ReadTConfig

        Dim configFilePath As String = GetConfigFilePath()

        ' Check if the config file exists
        If Not File.Exists(configFilePath) Then
            Throw New FileNotFoundException($"Config file not found at {configFilePath}")
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
    Public Sub WriteTConfig(Optional directoryPath As String = Nothing) Implements IUtility.WriteTConfig

        ' Check if a specific directoryPath is provided and use that, otherwise use the current directory
        If String.IsNullOrEmpty(directoryPath) Then
            directoryPath = Directory.GetCurrentDirectory()
        End If

        ' check if the directoryPath exists
        If String.IsNullOrEmpty(directoryPath) OrElse Not Directory.Exists(directoryPath) Then
            '    rootPathsFromConfig.Add(directoryPath)
            'Else
            Throw New DirectoryNotFoundException($"Directory does not exist: {directoryPath}")
        End If

        ' Get the path to the .tconfig file
        Dim configFilePath As String = GetConfigFilePath()

        ' Check if the config file exists
        If Not File.Exists(configFilePath) Then
            Throw New FileNotFoundException($"Config file not found at {configFilePath}")
        End If

        'Dim rootPathsFromConfig As New List(Of String)
        ' Read all lines from the .tconfig file
        'rootPathsFromConfig.AddRange(File.ReadAllLines(configFilePath))

        Dim rootPathsFromConfig = File.ReadAllLines(configFilePath).ToList

        ' Add the directory to the .tconfig file if it's not already in it
        If Not rootPathsFromConfig.Contains(directoryPath) Then
            rootPathsFromConfig.Add(directoryPath)
            File.WriteAllLines(configFilePath, rootPathsFromConfig)
        End If

    End Sub

    ''' <summary>
    ''' This method adds the source file (full path) to the .transclude file if it's not already in it and is "of interest".
    ''' </summary>
    ''' <param name="filePath">Full path of source file to be added to the `.transclude` file.</param>
    Public Sub AddSpecificFileToTransclude(filePath As String) Implements IUtility.AddSpecificFileToTransclude

        Dim directoryPath As String

        ' Verify that the source file exists
        If Not String.IsNullOrEmpty(filePath) AndAlso File.Exists(filePath) Then
            directoryPath = GetProperDirectoryPath(filePath)
        Else
            Throw New FileNotFoundException($"File path is null or empty or file does not exist: {filePath}")
        End If

        ' Add the source file to the .transclude file
        If _fileSync.IsFileOfInterest(filePath) AndAlso Not IsInstallDir(GetProperDirectoryPath(filePath)) Then
            WriteTranscludeFile(filePath)
        End If
    End Sub

    ''' <summary>
    ''' This method adds all files in the directory to the .transclude file that aren't already in it and are "of interest".
    ''' </summary>
    ''' <param name="directoryPath">Full path of the directory containing the .transclude file.</param>
    Public Sub AddAllFilesInDirectoryToTransclude(directoryPath As String) Implements IUtility.AddAllFilesInDirectoryToTransclude

        ' Check if directory exists
        If Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException($"Directory does not exist: {directoryPath}")
        End If

        ' Get all files in the directory
        Dim filesInDirectory As String() = Directory.GetFiles(directoryPath)

        ' Add all files that aren't already in the .transclude file and are "of interest"
        For Each filePath As String In filesInDirectory
            If _fileSync.IsFileOfInterest(filePath) AndAlso Not IsInstallDir(GetProperDirectoryPath(filePath)) Then
                WriteTranscludeFile(filePath)
            End If
        Next
    End Sub

    ''' <summary>
    ''' This method adds all files in the current directory to the .transclude file that aren't already in it and are "of interest".
    ''' </summary>
    Public Sub AddFilesInCurrentDirectoryToTransclude()
        Dim currentDirectoryPath As String = Directory.GetCurrentDirectory()

        ' Get all files in the directory
        Dim filesInDirectory As String() = Directory.GetFiles(currentDirectoryPath)

        ' Add all files that aren't already in the .transclude file and are "of interest"
        For Each filePath As String In filesInDirectory
            If _fileSync.IsFileOfInterest(filePath) AndAlso Not IsInstallDir(GetProperDirectoryPath(filePath)) Then
                WriteTranscludeFile(filePath)
            End If
        Next
    End Sub

    ''' <summary>
    ''' This method returns the full path to the .tconfig file.
    ''' </summary>
    ''' <remarks>
    ''' Method will throw an exception if the config file doesn't exist.
    ''' </remarks>
    ''' <returns></returns>
    Public Function GetConfigFilePath() As String

        Dim filePath As String

        Dim pathFromConfig As String = _options.Value.AdminSettings.ConfigFilePath

        Dim configFilePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tconfig")

        If Not String.IsNullOrEmpty(pathFromConfig) Then

            ' Use the path from the config file if it's not null or empty
            filePath = pathFromConfig
        Else

            ' Otherwise use the default path
            filePath = configFilePath
        End If

        ' Check if the config file exists
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"Config file not found at {filePath}")
        End If

        Return filePath
    End Function

    'Public Sub ManualSync(Optional directoryToSync As String = Nothing) Implements IUtility.ManualSync

    '    Dim directoriesToSync As New List(Of String)

    '    ' Check if a specific directory is provided and use that, otherwise use all directories in the config file
    '    If Not String.IsNullOrEmpty(directoryToSync) Then
    '        directoriesToSync.Add(directoryToSync)
    '    Else
    '        directoriesToSync = ReadTConfig()
    '    End If

    '    ' Filter out the application directory
    '    directoriesToSync = FilterInstallDirFromList(directoriesToSync)

    '    ' Catch empty list
    '    If directoriesToSync.Count = 0 Then
    '        _logger.LogWarning("{Method}: No root paths found.", NameOf(ManualSync))
    '        Return
    '    End If

    '    ' Loop through all directories in the .tconfig file
    '    For Each syncDir In directoriesToSync

    '        ' Check if the directory exists
    '        If Not String.IsNullOrEmpty(syncDir) AndAlso Directory.Exists(syncDir) Then

    '            Dim transcludeFiles As List(Of String)

    '            ' Check if the .transclude file exists
    '            If ExistsTranscludeFileAtPath(syncDir) Then 'Path.Exists Then 'ExistsFileInTransclude(syncDir) Then
    '                ' Read all files from the .transclude file
    '                transcludeFiles = ReadTranscludeFile(syncDir)
    '            Else
    '                'Go to next directory if there are no files in the .transclude file
    '                _logger.LogInformation("{Method}: No `.transclude` file found in directory: {Directory}", NameOf(ManualSync), syncDir)
    '                Continue For
    '            End If

    '            ' Sync all files in the .transclude file
    '            For Each filePath In transcludeFiles
    '                Try
    '                    If File.Exists(filePath) AndAlso _fileSync.IsFileOfInterest(filePath) Then
    '                        _fileSync.SyncSourceToMarkdown(filePath)
    '                    End If
    '                Catch ex As Exception
    '                    _logger.LogInformation("{Method}: Error syncing file: {FilePath}", NameOf(ManualSync), filePath)
    '                End Try
    '            Next

    '        Else
    '            _logger.LogInformation("{Method}: Directory does not exist: {Directory}", NameOf(ManualSync), syncDir)
    '        End If

    '    Next
    'End Sub

    Public Sub AdhocTranscode(Optional directoryToSync As String = Nothing) Implements IUtility.AdhocTranscode
        Dim directoriesToSync As New List(Of String)

        ' Check if a specific directory is provided, otherwise use all directories in the config file
        If Not String.IsNullOrEmpty(directoryToSync) Then
            directoriesToSync.Add(directoryToSync)
        Else
            directoriesToSync = ReadTConfig()
        End If

        ' Filter out the application directory
        directoriesToSync = FilterInstallDirFromList(directoriesToSync)

        ' Catch empty list
        If directoriesToSync.Count = 0 Then
            _logger.LogWarning("{Method}: No root paths found.", NameOf(AdhocTranscode))
            Return
        End If

        For Each syncDir In directoriesToSync
            ' Recursively process each directory
            RecurseSourceDirectories(syncDir)
        Next
    End Sub

    Private Sub RecurseSourceDirectories(dirPath As String)

        Dim existsTranscludeFile As Boolean = ExistsTranscludeFileAtPath(dirPath)

        ' Check if the directory exists
        If Not String.IsNullOrEmpty(dirPath) AndAlso Directory.Exists(dirPath) Then

            ' Check if the directory contains a .transclude file and is not the application directory
            If existsTranscludeFile AndAlso Not IsInstallDir(dirPath) Then
                ' Process files in the current directory
                TranscodeFilesInDirectory(dirPath)
            Else
                _logger.LogDebug("{Method}: Skipping directory because it does not contain a `.transclude` file: {Directory}", NameOf(RecurseSourceDirectories), dirPath)
            End If

            ' Get and process all subdirectories
            Dim subDirectories As String() = Directory.GetDirectories(dirPath)
            For Each subDir In subDirectories
                RecurseSourceDirectories(subDir)
            Next
        Else
            _logger.LogInformation("{Method}: Directory does not exist: {Directory}", NameOf(RecurseSourceDirectories), dirPath)
        End If
    End Sub

    Private Sub TranscodeFilesInDirectory(dirPath As String)
        Dim transcludeFiles As List(Of String) = ReadTranscludeFile(dirPath)

        Dim tcodeCount As Integer = 0

        ' Sync all files in the .transclude file
        For Each filePath In transcludeFiles
            Try
                If File.Exists(filePath) AndAlso _fileSync.IsFileOfInterest(filePath) Then
                    _fileSync.SyncSourceToMarkdown(filePath)
                    tcodeCount += 1
                End If
            Catch ex As Exception
                _logger.LogInformation("{Method}: Error transcoding file: {FilePath}", NameOf(TranscodeFilesInDirectory), filePath)
            End Try
        Next

        _logger.LogDebug("{Method}: {TranscodeFileCount} files transcoded in directory: {Directory}", NameOf(TranscodeFilesInDirectory), tcodeCount, dirPath)

    End Sub



    ''' <summary>
    ''' This method checks if the .transclude file exists in the directory.
    ''' </summary>
    ''' <param name="filePath">Full path of the directory to check.</param>
    ''' <returns></returns>
    Public Function ExistsTranscludeFileAtPath(filePath As String) As Boolean Implements IUtility.ExistsTranscludeFileAtPath
        Dim exists As Boolean = False

        Dim dirPath As String = GetProperDirectoryPath(filePath)

        Dim transcludeFilePath As String = Path.Combine(dirPath, ".transclude")

        Try
            exists = File.Exists(transcludeFilePath)
        Catch ex As Exception
            _logger.LogError(ex, "{Method}: Error checking if file exists: {FilePath}", NameOf(ExistsTranscludeFileAtPath), filePath)
        End Try

        Return exists
    End Function


    ''' <summary>
    ''' This method filters out the application directory from the list of directories.
    ''' </summary>
    ''' <param name="directories">List of directories to filter.</param>
    ''' <returns></returns>

    ' Filter out the application directory
    Public Function FilterInstallDirFromList(directories As List(Of String)) As List(Of String) Implements IUtility.FilterInstallDirFromList

        Dim appDir As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

        Dim filteredDirectories = directories.Where(Function(dir) Not dir.Equals(appDir, StringComparison.OrdinalIgnoreCase)).ToList()

        If directories.Count <> filteredDirectories.Count Then
            _logger.LogWarning("{Method}: The Application Directory {AppDir} was filtered out of the list of directories.", NameOf(FilterInstallDirFromList), appDir)
        End If

        Return filteredDirectories

    End Function

    ''' <summary>
    ''' This method checks if the directory path is the same as the application directory.
    ''' </summary>
    ''' <param name="directoryPath">Full path of the directory to check.</param>
    ''' <returns></returns>
    Public Function IsInstallDir(directoryPath As String) As Boolean Implements IUtility.IsInstallDir
        Dim appDir As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

        ' Compare this snippet from TransCodeMD/Utility.vb:
        Return directoryPath.Equals(appDir, StringComparison.OrdinalIgnoreCase)
    End Function


    ''' <summary>
    ''' This method filters out subdirectories that are already covered by other directories in the list.
    ''' </summary>
    ''' <remarks>
    ''' This method will check if any directory in the list is a subdirectory of any other directory in the list.
    ''' If it is, it will remove the subdirectory from the list. Primarily used to by the monitor to avoid monitoring the same directory twice.
    ''' </remarks>
    ''' <param name="directories">List of directories to filter.</param>
    ''' <returns></returns>
    Public Function FilterRedundantDirectoriesFromList(directories As List(Of String)) As List(Of String) Implements IUtility.FilterRedundantDirectoriesFromList
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

    Public Sub ListAllRefInLocalTranscodeFile() Implements IUtility.ListAllRefInLocalTranscodeFile
        Dim currentDirectoryPath As String = Directory.GetCurrentDirectory()
        ' Lists the content of the .transclude file in the current directory

        Dim fileList As List(Of String)

        Try
            fileList = ReadTranscludeFile(currentDirectoryPath)
        Catch ex As FileNotFoundException
            _logger.LogInformation("{Method}: No `.transclude` file found in directory: {Directory}", NameOf(ListAllRefInLocalTranscodeFile), currentDirectoryPath)
        Catch ex As Exception
            _logger.LogError(ex, "{Method}: Error reading .transclude file in directory: {Directory}", NameOf(ListAllRefInLocalTranscodeFile), currentDirectoryPath)
            Return
        End Try

        If fileList.Count = 0 Then
            _logger.LogInformation("{Method}: No files found in `.transclude` file in directory: {Directory}", NameOf(ListAllRefInLocalTranscodeFile), currentDirectoryPath)
            Return
        End If

        For Each f In fileList
            '_logger.LogInformation("{Method}: {File}", NameOf(ListAllRefInLocalTranscodeFile), f)
            System.Console.WriteLine(f)
        Next
    End Sub

    Public Sub ListAllRefInConfigFile() Implements IUtility.ListAllRefInConfigFile

        Dim configFilePath As String '= GetConfigFilePath()

        'Test if the config file exists
        Try
            configFilePath = GetConfigFilePath()
        Catch ex As FileNotFoundException
            _logger.LogInformation("{Method}: No `.tconfig` file found in directory: {Directory}", NameOf(ListAllRefInConfigFile), configFilePath)
            Return
        Catch ex As Exception
            _logger.LogError(ex, "{Method}: Error reading .tconfig file in directory: {Directory}", NameOf(ListAllRefInConfigFile), configFilePath)
            Return
        End Try


        Dim fileList As List(Of String)

        Try
            fileList = ReadTConfig()
        Catch ex As Exception
            _logger.LogError(ex, "{Method}: Error reading .tconfig file in directory: {Directory}", NameOf(ListAllRefInConfigFile), configFilePath)
            Return
        End Try

        If fileList.Count = 0 Then
            _logger.LogInformation("{Method}: No files found in `.tconfig` file in directory: {Directory}", NameOf(ListAllRefInConfigFile), configFilePath)
            Return
        End If

        For Each f In fileList
            '_logger.LogInformation("{Method}: {File}", NameOf(ListAllRefInConfigFile), file)
            System.Console.WriteLine(f)
        Next
    End Sub


    ''' <summary>
    ''' Returns the correct directory path for a given file or directory path.
    ''' This function is a fix for the limitation of Path.GetDirectoryName, which 
    ''' does not accurately distinguish between file paths and directory paths.
    ''' </summary>
    ''' <param name="path">The file or directory path to evaluate.</param>
    ''' <returns>The directory path if the input is a directory, or the parent directory if the input is a file path.</returns>
    ''' <remarks>
    ''' In the .NET framework, Path.GetDirectoryName treats the last segment of a path as a file name and returns 
    ''' its directory. However, this behavior is not accurate when the path is actually a directory.
    ''' 
    ''' This custom function checks whether the provided path is a directory or a file:
    ''' - If it's a directory, it returns the directory path as is.
    ''' - If it's a file, it returns the directory containing the file.
    ''' - If the path doesn't exist, it makes an educated guess based on the presence of a file extension.
    ''' 
    ''' This approach ensures that the correct directory path is returned regardless of whether the 
    ''' path is to a file or a directory, making it more reliable for various filesystem operations.
    ''' </remarks>
    Public Function GetProperDirectoryPath(targetPath As String) As String Implements IUtility.GetProperDirectoryPath
        If String.IsNullOrEmpty(targetPath) Then
            Return String.Empty
        End If

        ' Check if the path is a directory
        If Directory.Exists(targetPath) Then
            ' It's a directory; return it as is
            Return targetPath
        ElseIf File.Exists(targetPath) Then
            ' It's a file; return the directory containing the file
            Return Path.GetDirectoryName(targetPath)
        Else
            ' If the path doesn't exist, make an educated guess
            ' If the path has an extension, assume it's a file; otherwise, treat it as a directory
            If Path.HasExtension(targetPath) Then
                Return Path.GetDirectoryName(targetPath)
            Else
                Return targetPath
            End If
        End If
    End Function


End Class
