Imports System.Collections.Concurrent

Namespace Utilities

    Public Interface ILogPropertyMgr
        Sub Add(key As String, value As Func(Of Object))
        Sub Add(key As String, value As String)
        Sub Drop(key As String)
        Function GetProperties() As IReadOnlyDictionary(Of String, Object)
        'Sub Add(key As String, ByRef value As Func(Of Object))
        'Sub Add(key As String, ByVal value As String)
        'Function GetProperties() As Dictionary(Of String, Object)

    End Interface

    ' This class manages log properties in a thread-safe manner.
    ' It allows adding and retrieving log properties to be used with logging operations.
    Public Class LogPropertyMgr
        Implements ILogPropertyMgr

        ' Thread-safe dictionary to store log properties.
        Private ReadOnly _logPropertyDict As ConcurrentDictionary(Of String, LogProperty)

        Public Sub New()
            ' Initialize the dictionary.
            _logPropertyDict = New ConcurrentDictionary(Of String, LogProperty)()
        End Sub

        ' Adds a log property with a dynamic value determined by a function.
        ' If the key already exists, the existing property is updated.
        Public Sub Add(key As String, value As Func(Of Object)) Implements ILogPropertyMgr.Add
            Dim prop As New LogProperty(key, value)
            ' Update or add the property as necessary.
            _logPropertyDict.AddOrUpdate(key, prop, Function(k, v) prop)
        End Sub

        ' Adds a log property with a static string value.
        ' If the key already exists, the existing property is updated.
        Public Sub Add(key As String, value As String) Implements ILogPropertyMgr.Add
            ' Wrap the string in a function for consistency with the dictionary values.
            Dim funcValue As Func(Of Object) = Function() value
            Dim prop As New LogProperty(key, funcValue)
            ' Update or add the property as necessary.
            _logPropertyDict.AddOrUpdate(key, prop, Function(k, v) prop)
        End Sub

        ' Removes a log property by its key.
        ' If the key exists, the property is removed.
        ' If the key does not exist, nothing happens.
        Public Sub Drop(key As String) Implements ILogPropertyMgr.Drop
            Dim logProperty As LogProperty = Nothing
            _logPropertyDict.TryRemove(key, logProperty)
        End Sub

        ' Retrieves all log properties as a dictionary.
        ' This is a snapshot of the current state of properties.
        Public Function GetProperties() As IReadOnlyDictionary(Of String, Object) Implements ILogPropertyMgr.GetProperties
            ' Convert the concurrent dictionary to a regular dictionary.
            Return _logPropertyDict.ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value.Value)
        End Function

        ' Nested private class representing a single log property.
        Private Class LogProperty
            Private ReadOnly _value As Func(Of Object)
            Public ReadOnly Property Key As String
            Public ReadOnly Property Value As Object
                Get
                    ' Invoke the function to get the current value, returning "Null" if it fails.
                    Return If(_value.Invoke, "Null")
                End Get
            End Property

            ' Constructor to create a new log property with a key and a function to determine its value.
            Public Sub New(scopeKey As String, scopeValue As Func(Of Object))
                Key = scopeKey
                _value = scopeValue
            End Sub
        End Class

    End Class
End Namespace
