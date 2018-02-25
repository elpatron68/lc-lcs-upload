Imports System.IO
Imports System.Threading
Imports NLog
Imports Renci.SshNet

Public Class Lcssh

    ''' <summary>
    ''' Upload LCS Script file
    ''' </summary>
    ''' <param name="address"></param>
    ''' <param name="username"></param>
    ''' <param name="password"></param>
    ''' <param name="filename"></param>
    ''' <returns></returns>
    Public Shared Function Uploadlcs(ByVal address As String, ByVal username As String, ByVal password As String, ByVal filename As String) As String
        Dim mylogger As Logger = LogManager.GetCurrentClassLogger()
        Dim result = String.Empty
        Dim ConnNfo As New PasswordConnectionInfo(address, username, password)
        Dim SshClient As SshClient = New SshClient(ConnNfo)
        Dim ShellStream As ShellStream
        Dim sLine As String = String.Empty
        Dim sShelloutput As String = String.Empty

        Try
            SshClient.Connect()
            ShellStream = SshClient.CreateShellStream("LANCOM", 0, 0, 0, 0, 1024)
            Dim ShellReader As StreamReader = New StreamReader(ShellStream)
            Dim ShellWriter As StreamWriter = New StreamWriter(ShellStream) With {
                .AutoFlush = True
            }

            Try
                Dim FileReader As StreamReader = New StreamReader(filename)
                While Not FileReader.EndOfStream
                    sLine = FileReader.ReadLine
                    ShellWriter.WriteLine(sLine)
                    sShelloutput += GetShellReader(ShellReader).ReadToEnd
                End While
                FileReader.Close()
            Catch ex As System.IO.FileNotFoundException
                Debug.WriteLine(ex.Message)
                result = "File not found"
            Finally

            End Try

            While ShellStream.Length = 0
                Thread.Sleep(500)
            End While

            sShelloutput += ShellReader.ReadToEnd

            SshClient.Disconnect()
            logger.Debug(sShelloutput)
        Catch ex As Exception
            mylogger.Warn(ex.Message)
            result = ex.Message
        Finally
            If SshClient.IsConnected Then
                SshClient.Disconnect()
            End If
        End Try

        'If sShelloutput.ToLower.Contains("wrong") Or sShelloutput.ToLower.Contains("error") Then
        '    result = "Router reports error"
        'End If

        If sShelloutput.Contains("Goodbye") Then
            result = "Transfer successful"
        End If
        Return result
    End Function

    ''' <summary>
    ''' Download LANCOM configuration file (*.LCF)
    ''' </summary>
    ''' <param name="sIPRouter"></param>
    ''' <param name="sPassword"></param>
    ''' <param name="sLCFFile"></param>
    ''' <param name="sUSername"></param>
    ''' <returns>"0" (no error); "-1" (coonection error)</returns>
    ''' <remarks></remarks>
    Public Shared Function Downloadlcf(sIPRouter As String, sPassword As String, sLCFFile As String,
                                       Optional ByVal sUSername As String = "root") As String
        Dim mylogger As Logger = LogManager.GetCurrentClassLogger()
        Dim sConfig As String = Sendcommand(sIPRouter, sPassword, "readconfig", sUSername)
        logger.Debug($"Send router command: {sConfig}")
        Dim sResult As String = "0"
        Using sWritefile As StreamWriter = New StreamWriter(sLCFFile, False)
            Try
                sWritefile.Write(sConfig)
                sWritefile.Close()
            Catch ex As Exception
                sResult = "-1"
            Finally

            End Try
        End Using
        logger.Debug($"Result: {sResult}")
        Downloadlcf = sResult
    End Function

    ''' <summary>
    ''' Sends single command to console
    ''' </summary>
    ''' <param name="sIPRouter"></param>
    ''' <param name="sPassword"></param>
    ''' <param name="sCommand"></param>
    ''' <param name="sUSername"></param>
    ''' <returns>"console output (no error); "-1" (coonection error)</returns>
    ''' <remarks></remarks>
    Public Shared Function Sendcommand(sIPRouter As String, sPassword As String, sCommand As String,
                                       Optional ByVal sUSername As String = "root") As String
        Dim ConnNfo As New PasswordConnectionInfo(sIPRouter, sUSername, sPassword)
        Dim sReturn As String = "0"

        Using sshclient = New SshClient(ConnNfo)
            Try
                sshclient.Connect()
                Using cmd = sshclient.CreateCommand(sCommand)
                    sReturn = cmd.Execute()
                End Using
                sshclient.Disconnect()

            Catch ex As Exception
                sReturn = "-1"
            Finally
                If sshclient.IsConnected Then
                    sshclient.Disconnect()
                End If
            End Try
        End Using
        Return sReturn
    End Function

    Private Shared Function GetShellReader(ShellReader As StreamReader) As StreamReader
        Return ShellReader
    End Function
End Class
