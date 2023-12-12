Imports NMaier.GetOptNet

Public Interface ICliManager
    Property RunAsMonitor As Boolean
    Property MonitorRootDir As Boolean
    Property DirectoryPath As String
    Property MonitorSourceFile As String
End Interface

Public Class CliManager
    Inherits GetOpt
    Implements ICliManager

    Public Sub New()

        ' Get the command line arguments, skipping the first one which is empty
        Dim myArgs = Environment.GetCommandLineArgs.Skip(1).ToArray

        Try
            Me.Parse(myArgs)
        Catch ex As Exception
            System.Console.WriteLine($"{NameOf(CliManager)}: Error parsing command line arguments: {myArgs}")
            Throw
        End Try

    End Sub


    <Argument("runasmonitor", HelpText:="Run as monitor")>
    <ShortArgument("m")>
    Public Property RunAsMonitor As Boolean Implements ICliManager.RunAsMonitor ' Flag to indicate that the application should run as a monitor

    <Argument("includedir", HelpText:="Add directory to config")>
    <ShortArgument("d")>
    Public Property MonitorRootDir As Boolean Implements ICliManager.MonitorRootDir ' Flag to indicate that the root directory should be monitored (add to .tconfig)

    <Argument("dirpath", HelpText:="Add directory path to .tconfig")>
    <ShortArgument("p")>
    Public Property DirectoryPath As String Implements ICliManager.DirectoryPath ' The path to the directory to be monitored

    <Argument("sourcefile", HelpText:="Add source file to .transclude")>
    <ShortArgument("s")>
    Public Property MonitorSourceFile As String Implements ICliManager.MonitorSourceFile ' The `source.file` to monitor (add local .transclude)


End Class
