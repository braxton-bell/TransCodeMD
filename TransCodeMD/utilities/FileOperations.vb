Imports System.IO

Namespace Utilities
    Public Class FileOperations
        Public Shared Function ReadToString(filePath As String) As String
            Try
                ' Validate that the file exists
                If Not System.IO.File.Exists(filePath) Then
                    Throw New FileNotFoundException("File not found.")
                End If

                ' Validate that the file is not binary
                If IsBinaryFile(filePath) Then
                    Throw New InvalidOperationException("The file is a binary file.")
                End If

                ' Read the file content
                Return System.IO.File.ReadAllText(filePath)
            Catch ex As Exception
                ' Handle exceptions
                ' Log the error or rethrow, depending on your error handling strategy
                Throw
            End Try
        End Function

        Private Shared Function IsBinaryFile(filePath As String) As Boolean
            ' This is a basic check. More complex validation can be done based on requirements
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Dim bytes(1024) As Byte
                Dim bytesRead = stream.Read(bytes, 0, bytes.Length)

                For i = 0 To bytesRead - 1
                    If bytes(i) = 0 Then ' Null byte suggests a binary file
                        Return True
                    End If
                Next
            End Using

            Return False
        End Function
    End Class
End Namespace
