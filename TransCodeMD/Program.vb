Imports System.Threading
Imports TransCodeMD.Utilities
Module Program

    Sub Main(args As String())

        'Console.WriteLine("Hello World!")

        Call AllowSingleInstance()

    End Sub

    Private Sub AllowSingleInstance()

        Dim mutexId As String = "MyUniqueApplicationName"
        Dim createdNew As Boolean

        Using mutex As New Mutex(False, mutexId, createdNew)
            If createdNew Then
                ' The application is not already running
                'Console.WriteLine("Application started.")

                Dim startapp As New Startup

                Dim resultTask = startapp.Start()

                Dim result = resultTask.GetAwaiter.GetResult()

                If result.Success Then
                    Console.WriteLine("Application returned Success")
                Else
                    Console.WriteLine("Application Failed to Start")
                End If

                'startapp.Start.GetAwaiter.GetResult()

            Else
                ' The application is already running
                Console.WriteLine("An instance of the application is already running.")
            End If
        End Using
    End Sub

End Module