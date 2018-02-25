Imports System.IO
Imports NLog
Imports BurnSystems.CommandLine

Module Module1
    Public logger As Logger = LogManager.GetCurrentClassLogger()

    Sub Main()
        Dim routerfromcommandline As Routerinfo = New Routerinfo
        Dim filename = String.Empty
        Dim waitforuser As Boolean = False

        Console.WriteLine("LC-LCS-Upload v1.0")
        Console.WriteLine("(c) 2018 m.busche@gmail.com")
        Console.WriteLine("Free Open Source Software, see License.txt")
        Console.WriteLine()

        Dim parser = New Parser(My.Application.CommandLineArgs.ToArray) _
            .WithArgument("input", hasValue:=True, shortName:="i"c, isRequired:=True, helpText:="LANCOM script file (*.lcs)") _
            .WithArgument("address", hasValue:=True, shortName:="a"c, isRequired:=False, helpText:="Router address (IP or DN)") _
            .WithArgument("username", hasValue:=True, shortName:="u"c, defaultValue:="root", isRequired:=False, helpText:="SSH username, default: root") _
            .WithArgument("password", hasValue:=True, shortName:="p"c, isRequired:=False, helpText:="Ssh password") _
            .WithArgument("wait", hasValue:=False, shortName:="w"c, isRequired:=False, defaultValue:="0", helpText:="Wait for user after run") _
            .WithArgument("backup", hasValue:=True, shortName:="b"c, isRequired:=False, helpText:="Download LCF backup in <path> before uploading the script") _
            .WithArgument("help", hasValue:=False, shortName:="h"c, isRequired:=False, helpText:="Show help")


        If parser.ParseOrShowUsage() Then
            Try
                routerfromcommandline.Address = parser.NamedArguments("address")
            Catch ex As KeyNotFoundException
                logger.Info("No router adress given as command line argument")
            End Try

            Try
                routerfromcommandline.Username = parser.NamedArguments("username")
            Catch ex As KeyNotFoundException
                logger.Info("No username  given as command line argument")
            End Try
            Try
                routerfromcommandline.Password = parser.NamedArguments("password")
            Catch ex As KeyNotFoundException
                logger.Info("No password given as command line argument")
            End Try
            Try
                filename = parser.NamedArguments("input")
            Catch ex As KeyNotFoundException
                logger.Error("No filename given as command line argument")
                Exit Sub
            End Try
            Try
                routerfromcommandline.Backup = parser.NamedArguments("backup")
            Catch ex As KeyNotFoundException
                logger.Info("No backup directory given as command line argument")
            End Try
            If parser.NamedArguments("wait") = "1" Then
                waitforuser = True
            End If
        End If
        If filename = String.Empty Then
            Exit Sub
        End If

        logger.Info($"Script filename: {filename}")

        Dim routerfromfile As Routerinfo = GetDataFromFile(filename)
        Dim router As Routerinfo = New Routerinfo

        If routerfromcommandline.Address <> String.Empty Then
            router.Address = routerfromcommandline.Address
            logger.Info($"Using address from commandline: {router.Address}")
        ElseIf routerfromfile.Address <> String.Empty Then
            router.Address = routerfromfile.Address
            logger.Info($"Using address from file: {router.Address}")
        Else
            Console.WriteLine("Router address (IP or domain name)? ")
            router.Address = Console.ReadLine()
            logger.Info($"Using address from user input: {router.Address}")
        End If

        If routerfromcommandline.Username <> String.Empty Then
            router.Username = routerfromcommandline.Username
            logger.Info($"Using username from commandline: {router.Username}")
        ElseIf routerfromfile.Username <> String.Empty Then
            router.Username = routerfromfile.Username
            logger.Info($"Using username from file: {router.Username}")
        Else
            router.Username = "root"
            logger.Info($"Using default username (root): {router.Username}")
        End If

        If routerfromcommandline.Password <> String.Empty Then
            router.Password = routerfromcommandline.Password
            logger.Info($"Using password from commandline: {router.Password}")
        ElseIf routerfromfile.Password <> String.Empty Then
            router.Password = routerfromfile.Password
            logger.Info($"Using password from file: {router.Password}")
        Else
            Console.WriteLine("Password? ")
            router.Password = Console.ReadLine()
            logger.Info($"Using password from user input: {router.Password}")
        End If

        ' Create backup
        If routerfromcommandline.Backup <> String.Empty Then
            router.Backup = routerfromcommandline.Backup
            If Backuprouter(router) = False Then
                ' TODO: Exit if secure backup
            End If
        End If

        Dim result = Lcssh.Uploadlcs(router.Address, router.Username, router.Password, filename)
        If result <> String.Empty Then
            logger.Warn(result)
        Else
            logger.Info($"Script '{filename}' successfully uploaded to '{router.Address}'")
        End If


        If waitforuser = True Then
            Console.WriteLine("Hit any key to exit.")
            Console.ReadKey()
        End If
    End Sub

    ''' <summary>
    ''' Backup router setup as LCF file
    ''' </summary>
    ''' <param name="router"></param>
    ''' <returns></returns>
    Private Function Backuprouter(router As Routerinfo) As Boolean
        If Not Directory.Exists(router.Backup) Then
            Try
                logger.Info($"Creating backup directory: {router.Backup}")
                Directory.CreateDirectory(router.Backup)
            Catch ex As Exception
                logger.Fatal($"Failed creating backup directory: {ex.Message}")
                Backuprouter = False
                Exit Function
            End Try
        End If
        Dim backupfilename As String = $"{router.Backup}\\{router.Address}.lcf"
        If File.Exists(backupfilename) Then
            Try
                logger.Info($"Deleting exiting backup file: {backupfilename}")
                File.Delete(backupfilename)
            Catch ex As Exception
                logger.Error("Failed deleting exiting backup.")
                Backuprouter = False
                Exit Function
            End Try
        End If
        logger.Info("Downloading LCF backup from router")
        If Lcssh.Downloadlcf(router.Address, router.Password, backupfilename, router.Username) <> "-1" Then
            If File.Exists(backupfilename) Then
                Dim myFile As New FileInfo(backupfilename)
                logger.Info($"Backup reported no error. Filesize: {myFile.Length}")
                If myFile.Length > 200000 Then
                    Backuprouter = True
                    Exit Function
                End If
            Else
                logger.Warn("Backup file does not exist or file size too small")
            End If
        End If
        Backuprouter = False
    End Function

    ''' <summary>
    ''' Get settings from LCS file
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <returns></returns>
    Private Function GetDataFromFile(ByVal filename As String) As Routerinfo
        Dim result = New Routerinfo
        Dim line As String = String.Empty
        Using srLCS As StreamReader = New StreamReader(filename)
            While Not srLCS.EndOfStream
                line = srLCS.ReadLine
                If line.Contains("routeraddress:") Then
                    Try
                        line = line.Split(CChar(":"))(1).Trim
                        logger.Debug($"Router adress from file: {line}")
                        result.Address = line
                    Catch ex As Exception
                        logger.Warn($"No router address found in file.")
                    End Try
                End If
                If line.Contains("username:") Then
                    Try
                        line = line.Split(CChar(":"))(1).Trim
                        logger.Info($"Username from file: {line}")
                        result.Username = line
                    Catch ex As Exception
                        logger.Warn($"No username found in file.")
                    End Try
                End If
                If line.Contains("password:") Then
                    Try
                        line = line.Split(CChar(":"))(1).Trim
                        logger.Info($"Password from file: {line}")
                        result.Password = line
                    Catch ex As Exception
                        logger.Warn($"No passwort found in file.")
                    End Try
                End If
            End While
        End Using
        Return result
    End Function
End Module
