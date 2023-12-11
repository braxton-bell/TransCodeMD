Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports NMaier.GetOptNet
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Public Interface ICliManager
    Property RunAsMonitor As Boolean
End Interface

Public Class CliManager
    Inherits GetOpt
    Implements ICliManager

    'Private ReadOnly _options As IOptions(Of ApplicationConfig)
    'Private ReadOnly _propMgr As ILogPropertyMgr
    'Private ReadOnly _logger As ILogger(Of CliManager)

    'Public Sub New(options As IOptions(Of ApplicationConfig), propMgr As ILogPropertyMgr, logger As ILogger(Of CliManager))
    Public Sub New()

        ' Get the command line arguments, skipping the first one which is empty
        Dim myArgs = Environment.GetCommandLineArgs.Skip(1).ToArray

        Try
            Me.Parse(myArgs)
        Catch ex As Exception
            '_logger.LogError(ex, "{Method}: Error parsing command line arguments: {CommandLineArguments}", NameOf(CliManager), myArgs)
            System.Console.WriteLine($"{NameOf(CliManager)}: Error parsing command line arguments: {myArgs}")
            Throw
        End Try

        '_options = Options
        '_propMgr = propMgr
        '_logger = Logger

    End Sub


    <Argument("runasmonitor", HelpText:="Run as monitor")>
    <ShortArgument("m")>
    Public Property RunAsMonitor As Boolean Implements ICliManager.RunAsMonitor
End Class
