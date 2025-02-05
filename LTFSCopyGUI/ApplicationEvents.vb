﻿Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' 以下事件可用于 MyApplication: 
    ' Startup:应用程序启动时在创建启动窗体之前引发。
    ' Shutdown:在关闭所有应用程序窗体后引发。如果应用程序非正常终止，则不会引发此事件。
    ' UnhandledException:在应用程序遇到未经处理的异常时引发。
    ' StartupNextInstance:在启动单实例应用程序且应用程序已处于活动状态时引发。 
    ' NetworkAvailabilityChanged:在连接或断开网络连接时引发。
    Partial Friend Class MyApplication
        <System.Runtime.InteropServices.DllImport("kernel32.dll")>
        Public Shared Function AllocConsole() As Boolean

        End Function
        <System.Runtime.InteropServices.DllImport("kernel32.dll")>
        Shared Function FreeConsole() As Boolean

        End Function
        <System.Runtime.InteropServices.DllImport("kernel32.dll")>
        Shared Function AttachConsole(pid As Integer) As Boolean

        End Function
        Public Sub InitConsole()
            If Not AttachConsole(-1) Then
                AllocConsole()
            Else
                Dim CurrentLine As Integer = Console.CursorTop
                Console.SetCursorPosition(0, Console.CursorTop)
                Console.Write("".PadRight(Console.WindowWidth))
                Console.SetCursorPosition(0, CurrentLine - 1)
            End If
        End Sub
        Public Sub CloseConsole()
            System.Windows.Forms.SendKeys.SendWait("{ENTER}")
            FreeConsole()
        End Sub
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            My.Settings.License = " 非商业许可"
            If My.Computer.FileSystem.FileExists(My.Computer.FileSystem.CombinePath(System.Windows.Forms.Application.StartupPath, "license.key")) Then
                Dim rsa As New System.Security.Cryptography.RSACryptoServiceProvider()
                If My.Computer.FileSystem.FileExists(My.Computer.FileSystem.CombinePath(System.Windows.Forms.Application.StartupPath, "privkey.xml")) Then
                    rsa.FromXmlString(My.Computer.FileSystem.ReadAllText(My.Computer.FileSystem.CombinePath(System.Windows.Forms.Application.StartupPath, "privkey.xml")))
                Else
                    rsa.FromXmlString("<RSAKeyValue><Modulus>4q9IKAIqJVyJteY0L7mCVnuBvNv+ciqlJ79X8RdTOzAOsuwTrmdlXIJn0dNsY0EdTNQrJ+idmAcMzIDX65ZnQzMl9x2jfvLZfeArqzNYERkq0jpa/vwdk3wfqEUKhBrGzy14gt/tawRXp3eBGZSEN++Wllh8Zqf8Huiu6U+ZO9k=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>")
                End If
                Dim lic_string = My.Computer.FileSystem.ReadAllText(My.Computer.FileSystem.CombinePath(Windows.Forms.Application.StartupPath, "license.key"))
                Try
                    Dim key As Byte() = Convert.FromBase64String(lic_string)
                    lic_string = System.Text.Encoding.UTF8.GetString(rsa.Decrypt(key, False))
                    My.Settings.License = lic_string
                Catch ex As Exception
                    If rsa.PublicOnly Then
                        MessageBox.Show("许可证无效")
                    Else
                        My.Settings.License = lic_string
                        lic_string = Convert.ToBase64String(rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(lic_string), False))
                        My.Computer.FileSystem.WriteAllText(My.Computer.FileSystem.CombinePath(Windows.Forms.Application.StartupPath, "license.key"), lic_string, False)
                    End If
                End Try
            End If
            If e.CommandLine.Count = 0 Then

            Else
                Dim param() As String = e.CommandLine.ToArray()
                Dim IndexRead As Boolean = True
                For i As Integer = 0 To param.Count - 1
                    If param(i).StartsWith("/") Then param(i) = "-" & param(i).TrimStart("/")

                    Select Case param(i)
                        Case "-s"
                            IndexRead = False
                        Case "-t"
                            If i < param.Count - 1 Then
                                Dim TapeDrive As String = param(i + 1)
                                If TapeDrive.StartsWith("TAPE") Then
                                    TapeDrive = "\\.\" & TapeDrive
                                ElseIf TapeDrive.StartsWith("\\.\") Then
                                    'Do Nothing
                                ElseIf TapeDrive = Val(TapeDrive).ToString Then
                                    TapeDrive = "\\.\TAPE" & TapeDrive
                                Else

                                End If
                                Dim LWF As New LTFSWriter With {.TapeDrive = TapeDrive, .OfflineMode = Not IndexRead}
                                Me.MainForm = LWF
                                Exit For
                            End If
                        Case "-f"
                            If i < param.Count - 1 Then
                                Dim indexFile As String = param(i + 1).TrimStart("""").TrimEnd("""")

                                If My.Computer.FileSystem.FileExists(indexFile) Then
                                    Dim LWF As New LTFSWriter With {.Barcode = "索引查看", .TapeDrive = "", .OfflineMode = True}
                                    Dim OnLWFLoad As New EventHandler(Sub()
                                                                          LWF.Invoke(Sub()
                                                                                         LWF.LoadIndexFile(indexFile, True)
                                                                                         LWF.ToolStripStatusLabel1.Text = "索引查看"
                                                                                     End Sub)
                                                                          RemoveHandler LWF.Load, OnLWFLoad
                                                                      End Sub
                                        )
                                    AddHandler LWF.Load, OnLWFLoad
                                    Me.MainForm = LWF
                                End If
                                Exit For
                            End If
                        Case "-c"
                            Me.MainForm = LTFSConfigurator
                            Exit For
                        Case "-rb"
                            InitConsole()
                            If i < param.Count - 1 Then
                                Dim TapeDrive As String = param(i + 1)
                                If TapeDrive.StartsWith("TAPE") Then
                                    TapeDrive = "\\.\" & TapeDrive
                                ElseIf TapeDrive.StartsWith("\\.\") Then
                                    'Do Nothing
                                ElseIf TapeDrive = Val(TapeDrive).ToString Then
                                    TapeDrive = "\\.\TAPE" & TapeDrive
                                Else

                                End If
                                Dim Barcode As String = TapeUtils.ReadBarcode(TapeDrive)
                                Console.WriteLine($"{TapeDrive}{vbCrLf}Barcode:{Barcode}")
                                CloseConsole()
                                End
                            End If
                        Case "-wb"
                            InitConsole()
                            If i < param.Count - 2 Then
                                Dim TapeDrive As String = param(i + 1)
                                If TapeDrive.StartsWith("TAPE") Then
                                    TapeDrive = "\\.\" & TapeDrive
                                ElseIf TapeDrive.StartsWith("\\.\") Then
                                    'Do Nothing
                                ElseIf TapeDrive = Val(TapeDrive).ToString Then
                                    TapeDrive = "\\.\TAPE" & TapeDrive
                                Else

                                End If
                                Dim Barcode As String = param(i + 2)
                                If TapeUtils.SetBarcode(TapeDrive, Barcode) Then
                                    Console.WriteLine($"{TapeDrive}{vbCrLf}Barcode->{TapeUtils.ReadBarcode(TapeDrive)}")
                                Else
                                    Console.WriteLine($"{TapeDrive}{vbCrLf}设置Barcode失败")
                                End If
                                CloseConsole()
                                End
                            End If
                        Case "-raw"
                            InitConsole()
                            If i < param.Count - 4 Then
                                Dim TapeDrive As String = param(i + 1)
                                If TapeDrive.StartsWith("TAPE") Then
                                    TapeDrive = "\\.\" & TapeDrive
                                ElseIf TapeDrive.StartsWith("\\.\") Then
                                    'Do Nothing
                                ElseIf TapeDrive = Val(TapeDrive).ToString Then
                                    TapeDrive = "\\.\TAPE" & TapeDrive
                                Else

                                End If
                                Dim cdb As Byte() = LTFSConfigurator.HexStringToByteArray(param(i + 2))
                                Dim data As Byte() = LTFSConfigurator.HexStringToByteArray(param(i + 3))
                                Dim dataDir As Integer = Val(param(i + 4))
                                Dim sense As Byte() = {}

                                If TapeUtils.SendSCSICommand(TapeDrive, cdb, data, dataDir, Function(s As Byte())
                                                                                                sense = s
                                                                                                Return True
                                                                                            End Function) Then
                                    Console.WriteLine($"{TapeDrive}
cdb:
{TapeUtils.Byte2Hex(cdb)}
param:
{TapeUtils.Byte2Hex(data)}
dataDir:{dataDir}

SCSI命令执行成功
sense:
{TapeUtils.Byte2Hex(sense)}
{TapeUtils.ParseSenseData(sense)}")
                                Else
                                    Console.WriteLine($"{TapeDrive}
cdb:
{TapeUtils.Byte2Hex(cdb)}
param:
{TapeUtils.Byte2Hex(data)}
dataDir:{dataDir}

SCSI命令执行失败")
                                End If

                                CloseConsole()
                                End
                            End If
                        Case "-gt"
                            If i < param.Count - 2 Then
                                Dim Num1 As Byte = Byte.Parse(param(i + 1))
                                Dim Num2 As Byte = Byte.Parse(param(i + 2))
                                InitConsole()
                                Console.WriteLine($"{TapeUtils.GX256.Times(Num1, Num2)}")
                                CloseConsole()
                                End
                            End If
                        Case "-crc"
                            If i < param.Count - 1 Then
                                Dim Num1 As Byte() = LTFSConfigurator.HexStringToByteArray(param(i + 1))
                                InitConsole()
                                Console.WriteLine($"{TapeUtils.Byte2Hex(TapeUtils.GX256.CalcCRC(Num1))}")
                                CloseConsole()
                                End
                            End If
                        Case Else
                            Try
                                InitConsole()
                                Console.WriteLine($"LTFSCopyGUI v{My.Application.Info.Version.ToString(3)}{My.Settings.License}
  -s                                            不要自动读取索引
  -t <drive>                                    直接读写
  ├  -t 0
  ├  -t TAPE0
  └  -t \\.\TAPE0
                                           
  -f <file>                                     查看本地索引文件：
  └   -f C:\tmp\ltfs\000000.schema
                                           
  -c                                            LTFSConfigurator
                                           
  -rb <drive>                                   读Barcode
  ├  -rb 0
  ├  -rb TAPE0
  └  -rb \\.\TAPE0
                                           
  -wb <drive> <barcode>                         写Barcode
  └  -wb TAPE0 A00123L5                     
                                           
  -raw <drive> <cdb> <param> <dataDir>          SCSI命令执行
  └  -raw TAPE0 ""34 00 00 00 00 00 00 00 00"" ""00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"" 1")

                                CloseConsole()
                                End

                            Catch ex As Exception
                                MessageBox.Show(ex.ToString)
                            End Try
                            'End
                    End Select
                Next
            End If
        End Sub
    End Class
End Namespace
