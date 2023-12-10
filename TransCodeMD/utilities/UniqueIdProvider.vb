Namespace Utilities
    Public Class UniqueIdProvider
        Private Shared ReadOnly _batchId As String
        Private Shared _statelessSequenceNumber As Integer
        Private Shared ReadOnly _statelessLockObject As New Object()

        Shared Sub New()
            ' Generate the batch ID only once when the class is loaded for the first time.
            _batchId = GenerateBatchID()
        End Sub

        Public Shared ReadOnly Property BatchId As String
            Get
                ' Return the same batch ID throughout the life of the application.
                Return _batchId
            End Get
        End Property

        Public Shared ReadOnly Property GuId As Guid
            Get
                Return Guid.NewGuid()
            End Get
        End Property

        Private Shared Function GenerateBatchID() As String
            ' Short GUID segment (4 characters)
            Dim guidPart As String = Guid.NewGuid().ToString().Substring(0, 4)

            ' Compact date format (e.g., YYMMDD, 6 characters)
            Dim datePart As String = DateTime.UtcNow.Date.ToString("yyMMdd")

            ' Combine the parts
            Return $"{guidPart}{datePart}"
        End Function

        Public Shared Function GenerateId() As String
            Dim currentDate As DateTime
            Dim currentSequence As Integer
            SyncLock _statelessLockObject
                currentDate = DateTime.UtcNow.Date
                _statelessSequenceNumber += 1
                currentSequence = _statelessSequenceNumber
            End SyncLock

            Return GenerateId(currentDate, currentSequence)
        End Function

        Private Shared Function GenerateId(seedDate As DateTime, sequenceNumber As Integer) As String
            ' Short GUID segment (4 characters)
            Dim guidPart As String = Guid.NewGuid().ToString().Substring(0, 4)

            ' Compact date format (e.g., YYMMDD, 6 characters)
            Dim datePart As String = seedDate.ToString("yyMMdd")

            ' Sequence number (up to 7 digits to accommodate up to 9999999)
            Dim sequencePart As String = sequenceNumber.ToString("D7") ' Pads with zeros up to 7 digits

            ' Combine the parts (Total: 17 characters)
            Return $"{guidPart}{datePart}{sequencePart}"
        End Function
    End Class

End Namespace