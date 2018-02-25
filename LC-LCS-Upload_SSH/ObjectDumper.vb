Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Text

Public Class ObjectDumper
    Private _level As Integer

    Private ReadOnly _indentSize As Integer

    Private ReadOnly _stringBuilder As StringBuilder

    Private ReadOnly _hashListOfFoundElements As List(Of Integer)

    Private Sub New(ByVal indentSize As Integer)
        _indentSize = indentSize
        _stringBuilder = New StringBuilder()
        _hashListOfFoundElements = New List(Of Integer)()
    End Sub

    Public Shared Function Dump(ByVal element As Object) As String
        Return Dump(element, 2)
    End Function

    Public Shared Function Dump(ByVal element As Object, ByVal indentSize As Integer) As String
        Dim instance = New ObjectDumper(indentSize)
        Return instance.DumpElement(element)
    End Function

    Private Function DumpElement(ByVal element As Object) As String
        If element Is Nothing OrElse TypeOf element Is ValueType OrElse TypeOf element Is String Then
            Write(FormatValue(element))
        Else
            Dim objectType = element.[GetType]()
            If Not GetType(IEnumerable).IsAssignableFrom(objectType) Then
                Write("{{{0}}}", objectType.FullName)
                _hashListOfFoundElements.Add(element.GetHashCode())
                _level += 1
            End If

            Dim enumerableElement = TryCast(element, IEnumerable)
            If enumerableElement IsNot Nothing Then
                For Each item As Object In enumerableElement
                    If TypeOf item Is IEnumerable AndAlso Not (TypeOf item Is String) Then
                        _level += 1
                        DumpElement(item)
                        _level -= 1
                    Else
                        If Not AlreadyTouched(item) Then DumpElement(item) Else Write("{{{0}}} <-- bidirectional reference found", item.[GetType]().FullName)
                    End If
                Next
            Else
                Dim members As MemberInfo() = element.[GetType]().GetMembers(BindingFlags.[Public] Or BindingFlags.Instance)
                For Each memberInfo In members
                    Dim fieldInfo = TryCast(memberInfo, FieldInfo)
                    Dim propertyInfo = TryCast(memberInfo, PropertyInfo)
                    If fieldInfo Is Nothing AndAlso propertyInfo Is Nothing Then Continue For
                    Dim type = If(fieldInfo IsNot Nothing, fieldInfo.FieldType, propertyInfo.PropertyType)
                    Dim value As Object = If(fieldInfo IsNot Nothing, fieldInfo.GetValue(element), propertyInfo.GetValue(element, Nothing))
                    If type.IsValueType OrElse type = GetType(String) Then
                        Write("{0}: {1}", memberInfo.Name, FormatValue(value))
                    Else
                        Dim isEnumerable = GetType(IEnumerable).IsAssignableFrom(type)
                        Write("{0}: {1}", memberInfo.Name, If(isEnumerable, "...", "{ }"))
                        Dim alreadyTouched = Not isEnumerable AndAlso alreadyTouched(value)
                        _level += 1
                        If Not alreadyTouched Then DumpElement(value) Else Write("{{{0}}} <-- bidirectional reference found", value.[GetType]().FullName)
                        _level -= 1
                    End If
                Next
            End If

            If Not GetType(IEnumerable).IsAssignableFrom(objectType) Then
                _level -= 1
            End If
        End If

        Return _stringBuilder.ToString()
    End Function

    Private Function AlreadyTouched(ByVal value As Object) As Boolean
        If value Is Nothing Then Return False
        Dim hash = value.GetHashCode()
        For i = 0 To _hashListOfFoundElements.Count - 1
            If _hashListOfFoundElements(i) = hash Then Return True
        Next

        Return False
    End Function

    Private Sub Write(ByVal value As String, ParamArray args As Object())
        Dim space = New String(" "c, _level * _indentSize)
        If args IsNot Nothing Then value = String.Format(value, args)
        _stringBuilder.AppendLine(space & value)
    End Sub

    Private Function FormatValue(ByVal o As Object) As String
        If o Is Nothing Then Return ("null")
        If TypeOf o Is DateTime Then Return ((CType(o, DateTime)).ToShortDateString())
        If TypeOf o Is String Then Return String.Format("""{0}""", o)
        If TypeOf o Is Char AndAlso CChar(o) = vbNullChar Then Return String.Empty
        If TypeOf o Is ValueType Then Return (o.ToString())
        If TypeOf o Is IEnumerable Then Return ("...")
        Return ("{ }")
    End Function
End Class

