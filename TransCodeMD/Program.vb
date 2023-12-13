Imports System.Threading
Imports TransCodeMD.Utilities
Module Program

    Sub Main(args As String())

        Dim cliMgr As New CliManager

        Dim startapp As New Startup


        If cliMgr.RunAsMonitor Then
            Call RunAsSingleServiceInstance(startapp)
        Else
            Call RunAsUtility(startapp)
        End If

    End Sub

    Private Sub RunAsSingleServiceInstance(entryPoint As Startup)

        Dim mutexId As String = "MyUniqueApplicationName"
        Dim createdNew As Boolean

        Using mutex As New Mutex(False, mutexId, createdNew)
            If createdNew Then
                ' The application is not already running

                Dim resultTask = entryPoint.Start()

                Dim result = resultTask.GetAwaiter.GetResult()

                If result.Success Then
                    Console.WriteLine("Application returned Success")
                Else
                    Console.WriteLine("Application Failed to Start")
                End If

            Else
                ' The application is already running
                Console.WriteLine("An instance of the application is already running.")
            End If
        End Using

    End Sub

    Private Sub RunAsUtility(entryPoint As Startup)

        Dim resultTask = entryPoint.Start()

        Dim result = resultTask.GetAwaiter.GetResult()

        If result.Success Then
            Console.WriteLine(0) ' Success
        Else
            Console.WriteLine("An error occurred while running the application.")
        End If

    End Sub

End Module