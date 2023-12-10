Namespace Utilities
    Public Interface IOperationResult
        ReadOnly Property ErrorMessage As String
        ReadOnly Property Exception As Exception
        ReadOnly Property Success As Boolean
    End Interface


    ''' <summary>
    ''' Represents the outcome of an operation with a success flag, an error message, and an exception detail.
    ''' This class is used as the return type for methods where it is necessary to understand if the 
    ''' operation was successful or if it failed, providing context about the failure when needed.
    ''' The properties of this class are read-only and set upon creation, making instances of this class immutable 
    ''' and thread-safe. This immutability is essential for concurrent operations, ensuring that the result 
    ''' state cannot be altered after it has been constructed, which allows for reliable and predictable behavior 
    ''' when handling results across different threads.
    ''' </summary>
    ''' <remarks>
    ''' Instances of this class should be created using the provided static factory methods: 
    ''' <see cref="OperationResult.Ok"/> to indicate success and <see cref="OperationResult.Fail"/> to indicate failure.
    ''' By doing so, it maintains consistency in how operation results are generated and consumed throughout the application.
    ''' </remarks>
    Public Class OperationResult
        Implements IOperationResult

        Public ReadOnly Property Success As Boolean Implements IOperationResult.Success
        Public ReadOnly Property ErrorMessage As String Implements IOperationResult.ErrorMessage ' Optional
        Public ReadOnly Property Exception As Exception Implements IOperationResult.Exception ' Optional

        Private Sub New(success As Boolean, Optional errorMessage As String = "", Optional exception As Exception = Nothing)
            Me.Success = success
            Me.ErrorMessage = errorMessage
            Me.Exception = exception
        End Sub

        Public Shared Function Ok() As OperationResult
            Return New OperationResult(True)
        End Function

        Public Shared Function Fail(Optional message As String = "", Optional ex As Exception = Nothing) As OperationResult
            Return New OperationResult(False, message, ex)
        End Function

    End Class

End Namespace

