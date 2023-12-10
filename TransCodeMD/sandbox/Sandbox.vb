Imports System.Threading
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports TransCodeMD.Config
Imports TransCodeMD.Utilities

Namespace Sandbox

    Public Interface ISandbox
        Sub Play()
    End Interface

    Public Class Sandbox
        Implements ISandbox

        'Private ReadOnly _log As LogHandler
        Private ReadOnly _logger As ILogger(Of Sandbox)
        Private ReadOnly _services As IServiceProvider
        Private ReadOnly _options As IOptions(Of ApplicationConfig)

        Public Sub New(logger As ILogger(Of Sandbox), services As IServiceProvider, options As IOptions(Of ApplicationConfig))

            _logger = logger
            _services = services
            _options = options

            _logger.LogInformation("Entering development sandbox... ")
        End Sub

        Public Sub Play() Implements ISandbox.Play

            PrintNum()

        End Sub

        Private Sub PrintNum_2()
            'https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.wait?view=net-7.0
            Dim max As Int32 = 10
            Dim counter = 0

            Dim myString = DoSomethingTestAsync()

            Do While counter < max

                _logger.LogInformation($"Counter: {counter}")
                Thread.Sleep(1000)
                counter = counter + 1
                If myString.IsCompleted Then _logger.LogInformation(myString.Result)
            Loop

            If myString.Wait(1000) Then
                _logger.LogInformation("Job Completed")
            Else
                _logger.LogInformation("Time out!")
            End If

        End Sub

        Private Async Function DoSomethingTestAsync() As Task(Of String)
            'https://learn.microsoft.com/en-us/dotnet/visual-basic/programming-guide/concepts/async/

            Dim t = Task(Of String).Run(Function()
                                            Thread.Sleep(12000)
                                            Return "test"
                                        End Function)


            Dim myString As String = Await t

            Return myString

        End Function

        Private Sub PrintNum()

            Dim max As Int32 = 10000
            Dim counter = 0

            Do While counter < max
                _logger.LogInformation("Counter: {counterVar}", counter)
                '_log.Handler($"Counter: {counter}", LogHandler.Impact.LogInformation)
                Thread.Sleep(1000)
                counter = counter + 1
            Loop

            _logger.LogInformation("Counter: {counterVar}", counter)
        End Sub

    End Class
End Namespace