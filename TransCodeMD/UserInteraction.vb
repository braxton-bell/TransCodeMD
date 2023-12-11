Imports System.Threading
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface IUserInteraction
    Function ConfirmSyncForNewerMarkdown() As Boolean
    Function ExitApplication() As Boolean
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

        'If System.Console.ReadKey().KeyChar = "q"c Then
        '    Return True
        'End If

        'Return True

        Return key.KeyChar = "q"c OrElse key.KeyChar = "Q"c

    End Function


End Class
