Imports SnmpSharpNet
Imports System.Net
Imports System.Net.Sockets
Imports MySql.Data.MySqlClient

Namespace cps_snmp_receiver
    Module Module1
        Dim ip_server, port_server, db_database, db_user, db_pass, status_db, dido_specific As String
        Public Sub Main()

            load_file_ini()
            Dim connectionString As String = "Server=" & ip_server & ";Database=" & db_database & ";Uid=" & db_user & ";Pwd=" & db_pass & ";"
            ' Parameter SNMP listening
            Dim port As Integer = 162
            Dim ip As String = "10.10.200.10"

            ' Membuat socket dan mengikatnya
            Dim socket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            Dim ipEndPoint As New IPEndPoint(System.Net.IPAddress.Parse(ip), port)
            socket.Bind(ipEndPoint)

            Console.WriteLine("CPS - SNMP Trap Receiver v1.0 dimulai...")
            Console.WriteLine($"Mendengarkan di {ip}:{port}")
            Console.WriteLine("Tekan Ctrl+C untuk berhenti")
            Console.WriteLine("")
            Console.WriteLine("----------------------------------------------------------------")
            Console.WriteLine(" FROM CPS IP ; DI/DO ; VALUE ;       TIMESTAMP        ; STATUS ;")
            Console.WriteLine("----------------------------------------------------------------")

            While True
                Try
                    ' Buffer untuk menerima data
                    Dim inData(16384) As Byte
                    Dim ipEndPointFrom As New IPEndPoint(System.Net.IPAddress.Any, 0)
                    Dim ep As EndPoint = ipEndPointFrom

                    ' Menerima trap
                    Dim inLength As Integer = socket.ReceiveFrom(inData, ep)

                    ' Mengkonversi endpoint yang diterima ke IPEndPoint
                    Dim ipEndPointReceived As IPEndPoint = DirectCast(ep, IPEndPoint)

                    Try
                        ' Memproses trap versi 1
                        Dim trap As New SnmpV1TrapPacket()
                        trap.decode(inData, inLength)

                        'Console.WriteLine("----------------------------------------")
                        'Console.WriteLine($"Trap diterima dari: {ipEndPointReceived.Address}")
                        'Console.WriteLine($"Community: {trap.Community}")
                        'Console.WriteLine($"Enterprise: {trap.Enterprise}")
                        'Console.WriteLine($"Generic Type: {trap.Pdu.Generic}")
                        'Console.WriteLine($"Specific Type: {trap.Pdu.Specific}")

                        If trap.Pdu.Specific = 1 Then
                            dido_specific = "DI-0"
                        ElseIf trap.Pdu.Specific = 2 Then
                            dido_specific = "DI-1"
                        ElseIf trap.Pdu.Specific = 3 Then
                            dido_specific = "DI-2"
                        ElseIf trap.Pdu.Specific = 4 Then
                            dido_specific = "DI-3"
                        ElseIf trap.Pdu.Specific = 5 Then
                            dido_specific = "DI-4"
                        ElseIf trap.Pdu.Specific = 6 Then
                            dido_specific = "DI-5"
                        ElseIf trap.Pdu.Specific = 17 Then
                            dido_specific = "DO-0"
                        ElseIf trap.Pdu.Specific = 18 Then
                            dido_specific = "DO-1"
                        ElseIf trap.Pdu.Specific = 19 Then
                            dido_specific = "DO-2"
                        ElseIf trap.Pdu.Specific = 20 Then
                            dido_specific = "DO-3"
                        ElseIf trap.Pdu.Specific = 21 Then
                            dido_specific = "DO-4"
                        ElseIf trap.Pdu.Specific = 22 Then
                            dido_specific = "DO-5"
                        End If

                        Try
                            ' Save to Database
                            Using connection As New MySqlConnection(connectionString)
                                connection.Open()
                                'Dim query As String = "INSERT INTO cps (dido, value, ip_cps, timestamp_all) VALUES (@specific, @value, @ip, @timestamp)"
                                Dim query As String = "INSERT INTO cps (dido, value, ip_cps, timestamp_all) VALUES (@specific, @value, @ip, @timestamp)"

                                Using cmd As New MySqlCommand(query, connection)
                                    'cmd.Parameters.AddWithValue("@specific", trap.Pdu.Specific)
                                    cmd.Parameters.AddWithValue("@specific", dido_specific)
                                    ' Get the value from VbList (first item)
                                    Dim trapValue As String = ""
                                    If trap.Pdu.VbList.Count > 0 Then
                                        trapValue = trap.Pdu.VbList(0).Value.ToString()
                                    End If
                                    cmd.Parameters.AddWithValue("@value", trapValue)
                                    cmd.Parameters.AddWithValue("@ip", ipEndPointReceived.Address.ToString())
                                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now)
                                    cmd.ExecuteNonQuery()
                                End Using
                                status_db = "Saved"
                            End Using
                        Catch ex As Exception
                            'Console.WriteLine(ex.Message)
                            status_db = "Failed"
                        End Try

                        ' Consolidated data output
                        Console.WriteLine($"{ipEndPointReceived.Address};  {dido_specific} ;   {If(trap.Pdu.VbList.Count > 0, trap.Pdu.VbList(0).Value.ToString(), "")}   ; {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")}; {status_db}  ;")

                        ' Menampilkan variable bindings
                        'Console.WriteLine($"VarBind count: {trap.Pdu.VbList.Count}")
                        'Console.WriteLine("VarBind content:")
                        For Each v As Vb In trap.Pdu.VbList
                            'Console.WriteLine($"OID: {v.Oid}, Type: {SnmpConstants.GetTypeName(v.Value.Type)}, Value: {v.Value}")
                            'Console.WriteLine($"Value: {v.Value}")
                        Next

                        'Console.WriteLine($"Time Stamp: {trap.Pdu.TimeStamp}")

                        ' Convert TimeStamp to datetime format
                        Dim currentDate As DateTime = DateTime.Now
                        Dim trapTime As DateTime = currentDate.AddMilliseconds(-(trap.Pdu.TimeStamp / 100))
                        'Console.WriteLine($"Time Stamp: {trapTime.ToString("dd-MM-yyyy HH:mm:ss.fff")}")

                    Catch ex As Exception
                        Console.WriteLine($"Error saat mendekode trap: {ex.Message}")
                    End Try

                Catch ex As Exception
                    Console.WriteLine($"Error saat menerima trap: {ex.Message}")
                End Try
            End While
        End Sub

        Public Sub load_file_ini()
            'utuk load file.ini
            Dim strINIFile As String = ".\config.ini"
            'penglangan
            Dim i As Integer
            i = 1
            Do While (i <= 8)
                'masukan prosesnya
                ip_server = GetIniSetting(strINIFile, "ip_server", 1)
                db_database = GetIniSetting(strINIFile, "ip_server", 2)
                db_user = GetIniSetting(strINIFile, "ip_server", 3)
                db_pass = GetIniSetting(strINIFile, "ip_server", 4)
                'port_server = GetIniSetting(strINIFile, "ip_server", 5)
                i += 1
            Loop
        End Sub
    End Module
End Namespace