Public Class Routerinfo
    Private rtAddress As String
    Private rtUser As String
    Private rtPassword As String
    Private rtBackup As String

    Public Sub New()
        Address = String.Empty
        Username = String.Empty
        Password = String.Empty
        Backup = String.Empty
    End Sub

    Public Property Address As String
        Get
            Return rtAddress
        End Get
        Set(value As String)
            rtAddress = value
        End Set
    End Property

    Public Property Username As String
        Get
            Return rtUser
        End Get
        Set(value As String)
            rtUser = value
        End Set
    End Property

    Public Property Password As String
        Get
            Return rtPassword
        End Get
        Set(value As String)
            rtPassword = value
        End Set
    End Property

    Public Property Backup As String
        Get
            Return rtBackup
        End Get
        Set(value As String)
            rtBackup = value
        End Set
    End Property

End Class
