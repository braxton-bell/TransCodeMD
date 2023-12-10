Namespace Utilities
    'Public Interface IExceptionTracker
    '    Sub TrackException(ex As Exception)
    '    Function GetExceptionDetails() As Dictionary(Of String, (Count As Integer, ExampleException As Exception))
    'End Interface

    ''' <summary>
    ''' A utility class to track exceptions and their occurrences.
    ''' </summary>
    Public Class ExceptionTracker
        'Implements IExceptionTracker

        ' Dictionary to keep track of exception types and their details (count and an example exception)
        Private exceptionDetails As New Dictionary(Of String, (Count As Integer, ExampleException As Exception))

        ''' <summary>
        ''' Tracks an exception, incrementing the count and storing an example if it's the first occurrence.
        ''' </summary>
        ''' <param name="ex">The exception to track.</param>
        Public Sub TrackException(ex As Exception) 'Implements IExceptionTracker.TrackException
            Dim exType As String = ex.GetType().ToString()
            If Not exceptionDetails.ContainsKey(exType) Then
                exceptionDetails.Add(exType, (0, ex))
            End If
            Dim currentDetails = exceptionDetails(exType)
            exceptionDetails(exType) = (currentDetails.Count + 1, currentDetails.ExampleException)
        End Sub

        ''' <summary>
        ''' Gets the details of all tracked exceptions.
        ''' </summary>
        ''' <returns>A dictionary of exception types and their details.</returns>
        Public Function GetExceptionDetails() As IReadOnlyDictionary(Of String, (Count As Integer, ExampleException As Exception)) 'Implements IExceptionTracker.GetExceptionDetails
            Return exceptionDetails
        End Function

        ''' <summary>
        ''' Factory method for creating an instance of ExceptionTracker.
        ''' </summary>
        ''' <returns>An instance of ExceptionTracker.</returns>
        Public Shared Function Create() As ExceptionTracker
            Return New ExceptionTracker()
        End Function

    End Class
End Namespace