Public Class Routerinfo
    Private _rtAddress As String
    Private _rtUser As String
    Private _rtPassword As String
    Private _rtBackup As String

    Public Sub New()
        Address = String.Empty
        Username = String.Empty
        Password = String.Empty
        Backup = String.Empty
    End Sub

    Public Property Address As String
        Get
            Return _rtAddress
        End Get
        Set(value As String)
            _rtAddress = value
        End Set
    End Property

    Public Property Username As String
        Get
            Return _rtUser
        End Get
        Set(value As String)
            _rtUser = value
        End Set
    End Property

    Public Property Password As String
        Get
            Return _rtPassword
        End Get
        Set(value As String)
            _rtPassword = value
        End Set
    End Property

    Public Property Backup As String
        Get
            Return _rtBackup
        End Get
        Set(value As String)
            _rtBackup = value
        End Set
    End Property

End Class
