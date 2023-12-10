Imports System
Imports System.IO

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")

        ' Directory to monitor - this should be replaced with your actual directory path
        Dim monitorDirectory As String = "C:\source\repos\TransCodeMD\demo"

        Using watcher As New FileSystemWatcher(monitorDirectory)
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
            Console.WriteLine("Press 'q' to quit the sample.")
            While Char.ToLowerInvariant(Console.ReadKey().KeyChar) <> "q"c
                System.Threading.Thread.Sleep(1000)
            End While
        End Using
    End Sub

    ' Define the event handlers.

    'Private Sub OnChanged(sender As Object, e As FileSystemEventArgs)
    '    ' Specify what is done when a file is changed, created, or deleted.
    '    Console.WriteLine($"File: {e.FullPath} {e.ChangeType}")
    'End Sub
    Private Sub OnChanged(sender As Object, e As FileSystemEventArgs)
        If IsFileOfInterest(e.FullPath) Then
            ' Implement your sync logic here
            Console.WriteLine($"File of interest changed: {e.FullPath} {e.ChangeType}")
        End If
    End Sub

    Private Sub OnRenamed(sender As Object, e As RenamedEventArgs)
        ' Specify what is done when a file is renamed.
        Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}")
    End Sub

    Private Function IsFileOfInterest(filePath As String) As Boolean
        Dim extensionsOfInterest As String() = {".py", ".cs", ".vb", ".ps1", ".md"}
        Dim fileExtension As String = Path.GetExtension(filePath)
        Return extensionsOfInterest.Contains(fileExtension.ToLower())
    End Function

End Module