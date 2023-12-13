Imports NMaier.GetOptNet

Public Interface ICliManager
    Property RunAsMonitor As Boolean
    Property AddRootPath As Boolean
    Property RootPath As String
    Property AddSourceFile As String
    Property ShowHelp As Boolean
    Property AddSourceFilesFromDirectory As Boolean
    Property SourceFilePath As String
    Property SyncFiles As Boolean
    Property SyncPath As String
End Interface

Public Class CliManager
    Inherits GetOpt
    Implements ICliManager

    Public Sub New()

        ' Get the command line arguments, skipping the first one which is empty
        Dim myArgs = Environment.GetCommandLineArgs.Skip(1).ToArray

        Try
            Me.Parse(myArgs)

            ' Check if help is requested
            If Me.ShowHelp Then
                Me.PrintUsage()
                Environment.Exit(0)
            ElseIf myArgs.Length = 0 Then
                Me.PrintUsage()
                Environment.Exit(0)
            End If

        Catch ex As UnknownAttributeException
            System.Console.WriteLine($"{NameOf(CliManager)}: An unknown attribute was specified: {ex.Message}")
            Environment.Exit(1)
        Catch ex As Exception
            System.Console.WriteLine($"{NameOf(CliManager)}: Error parsing command line arguments: {myArgs}")
            Environment.Exit(1)
        End Try

    End Sub


    <Argument("runasmonitor", HelpText:="Run in monitor mode. Only a single instance allowed.")>
    <ShortArgument("m")>
    Public Property RunAsMonitor As Boolean Implements ICliManager.RunAsMonitor ' Flag to indicate that the application should run as a monitor

    <Argument("addrootpath", HelpText:="Add current directory as a root path in `.tconfig`. Can be combined with `-p` to specify a different directory.")>
    <ShortArgument("d")>
    Public Property AddRootPath As Boolean Implements ICliManager.AddRootPath ' Flag to indicate that the root directory should be monitored (add to .tconfig)

    <Argument("dirpath", HelpText:="Specifies the directory to add as a root path in  `.tconfig`. Used with `-d`.")>
    <ShortArgument("p")>
    Public Property RootPath As String Implements ICliManager.RootPath ' The path to the directory to be monitored

    <Argument("sourcefile", HelpText:="Add a single source file to `.transclude`")>
    <ShortArgument("s")>
    Public Property AddSourceFile As String Implements ICliManager.AddSourceFile ' The `source.file` to monitor (add local .transclude)

    <Argument("addsourcefiles", HelpText:="Add all source files in the current directory to `.transclude`. Can be combined with `-f` to specify a different directory.")>
    <ShortArgument("a")>
    Public Property AddSourceFilesFromDirectory As Boolean Implements ICliManager.AddSourceFilesFromDirectory ' Flag to indicate that all source files in a directory should be added to the `.transclude` file.

    <Argument("sourefilepath", HelpText:="Specifies directory from which all sources files will be added to `.transclude`. Used with `-a`.")>
    <ShortArgument("f")>
    Public Property SourceFilePath As String Implements ICliManager.SourceFilePath

    <Argument("syncfiles", HelpText:="Sync all files in `.transclude`. Can be combined with `-z` to specify a specific directory path to sync.")>
    <ShortArgument("y")>
    Public Property SyncFiles As Boolean Implements ICliManager.SyncFiles ' Switch to execute a adhoc file sync operation for all files in the .transclude file.

    <Argument("syncpath", HelpText:="Specifies directory to sync. Used with `-y`.")>
    <ShortArgument("z")>
    Public Property SyncPath As String Implements ICliManager.SyncPath ' The path to the directory to be synced manually.

    <Argument("help", HelpText:="Show help information")>
    <ShortArgument("h")>
    Public Property ShowHelp As Boolean Implements ICliManager.ShowHelp ' Flag to show help information

End Class
