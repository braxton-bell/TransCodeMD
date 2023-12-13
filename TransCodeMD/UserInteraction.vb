Imports System.Threading
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUserInteraction
    Function ConfirmSyncForNewerMarkdown() As Boolean
    Function ExitApplication() As Boolean
    Function ConfirmAddDirectoryToConfig(directoryPath As String) As Boolean
    Function ConfirmAddSourceFilesToTransclude(directoryPath As String, Optional sourceFilePaths As String = Nothing) As Boolean
    Function ConfirmAdhocSync() As Boolean
End Interface

Public Class UserInteraction
    Implements IUserInteraction

    Private ReadOnly _options As IOptions(Of ApplicationConfig)
    Private ReadOnly _propMgr As ILogPropertyMgr
    Private ReadOnly _logger As ILogger(Of UserInteraction)

    Public Sub New(options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of UserInteraction))
        _options = options
        _propMgr = propMgr
        _logger = logger
    End Sub

    Public Function ConfirmSyncForNewerMarkdown() As Boolean Implements IUserInteraction.ConfirmSyncForNewerMarkdown
        System.Console.WriteLine("The Markdown file is newer than the source file. Do you want to overwrite it? [Y/N]")
        Dim key = System.Console.ReadKey()
        Return key.KeyChar = "Y"c OrElse key.KeyChar = "y"c
    End Function

    Public Function ExitApplication() As Boolean Implements IUserInteraction.ExitApplication

        System.Console.WriteLine("Press 'q' to exit the application.")

        Dim key = System.Console.ReadKey()

        Return key.KeyChar = "q"c OrElse key.KeyChar = "Q"c

    End Function

    Public Function ConfirmAddDirectoryToConfig(directoryPath As String) As Boolean Implements IUserInteraction.ConfirmAddDirectoryToConfig
        System.Console.WriteLine($"Do you want to add the directory '{directoryPath}' to the config? [Y/N]")
        Dim key = System.Console.ReadKey()
        Return key.KeyChar = "Y"c OrElse key.KeyChar = "y"c
    End Function

    Public Function ConfirmAddSourceFilesToTransclude(directoryPath As String, Optional sourceFilePath As String = Nothing) As Boolean Implements IUserInteraction.ConfirmAddSourceFilesToTransclude
        Dim key

        If sourceFilePath Is Nothing Then
            System.Console.WriteLine($"Do you want to add all source files to the .transclude file in '{directoryPath}'? [Y/N]")
            key = System.Console.ReadKey()

        Else
            System.Console.WriteLine($"Do you want to add the source file '{sourceFilePath}' to the .transclude file in '{directoryPath}'? [Y/N]")
            key = System.Console.ReadKey()
        End If

        Return key.KeyChar = "Y"c OrElse key.KeyChar = "y"c

    End Function

    Public Function ConfirmAdhocSync() As Boolean Implements IUserInteraction.ConfirmAdhocSync

        Dim key

        System.Console.WriteLine($"You are about to perform an adhoc sync. Do you want to continue? [Y/N]")
        key = System.Console.ReadKey()

        Return key.KeyChar = "Y"c OrElse key.KeyChar = "y"c

    End Function


End Class
