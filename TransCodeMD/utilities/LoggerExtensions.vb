Imports Microsoft.Extensions.Logging
Imports TransCodeMD.Utilities
Imports System.Runtime.CompilerServices

''' <summary>
''' Extension methods for ILogger interface to enhance logging capabilities.
''' </summary>
Module LoggerExtensions

    ''' <summary>
    ''' Logs the count and details of each type of exception tracked by the ExceptionTracker.
    ''' </summary>
    ''' <param name="logger">The logger instance to use for logging.</param>
    ''' <param name="tracker">The ExceptionTracker instance containing the tracked exceptions.</param>
    ''' <param name="context">The context in which the exceptions occurred (usually the method name).</param>
    <Extension()>
    Public Sub LogExceptionCounts(logger As ILogger, tracker As ExceptionTracker, context As String)
        For Each kvp As KeyValuePair(Of String, (Count As Integer, ExampleException As Exception)) In tracker.GetExceptionDetails()
            Dim exceptionType = kvp.Key
            Dim count = kvp.Value.Count
            Dim exampleException = kvp.Value.ExampleException

            logger.LogWarning($"Exception of type {exceptionType} occurred {count} times in {context}. Example exception: {exampleException.ToString()}")
        Next
    End Sub

End Module

