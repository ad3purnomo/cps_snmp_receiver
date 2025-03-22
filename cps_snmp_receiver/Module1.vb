Imports SnmpSharpNet
Imports System.Net
Imports System.Net.Sockets

Namespace cps_snmp_receiver
    Module Module1
        Public Sub Main()
            ' Parameter SNMP listening
            Dim port As Integer = 162
            Dim ip As String = "10.10.200.10"

            ' Membuat socket dan mengikatnya
            Dim socket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            Dim ipEndPoint As New IPEndPoint(System.Net.IPAddress.Parse(ip), port)
            socket.Bind(ipEndPoint)

            Console.WriteLine("SNMP Trap Receiver dimulai...")
            Console.WriteLine($"Mendengarkan di {ip}:{port}")
            Console.WriteLine("Tekan Ctrl+C untuk berhenti")

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

                        Console.WriteLine("----------------------------------------")
                        Console.WriteLine($"Trap diterima dari: {ipEndPointReceived.Address}")
                        'Console.WriteLine($"Community: {trap.Community}")
                        'Console.WriteLine($"Enterprise: {trap.Enterprise}")
                        'Console.WriteLine($"Generic Type: {trap.Pdu.Generic}")
                        Console.WriteLine($"Specific Type: {trap.Pdu.Specific}")
                        'Console.WriteLine($"Agent Address: {trap.Pdu.AgentAddress}")

                        ' Menampilkan variable bindings
                        'Console.WriteLine($"VarBind count: {trap.Pdu.VbList.Count}")
                        'Console.WriteLine("VarBind content:")
                        For Each v As Vb In trap.Pdu.VbList
                            'Console.WriteLine($"OID: {v.Oid}, Type: {SnmpConstants.GetTypeName(v.Value.Type)}, Value: {v.Value}")
                            Console.WriteLine($"Value: {v.Value}")
                        Next

                        'Console.WriteLine($"Time Stamp: {trap.Pdu.TimeStamp}")

                        ' Convert TimeStamp to datetime format
                        Dim currentDate As DateTime = DateTime.Now
                        Dim trapTime As DateTime = currentDate.AddMilliseconds(-(trap.Pdu.TimeStamp / 100))
                        Console.WriteLine($"Time Stamp: {trapTime.ToString("dd-MM-yyyy HH:mm")}")

                    Catch ex As Exception
                        Console.WriteLine($"Error saat mendekode trap: {ex.Message}")
                    End Try

                Catch ex As Exception
                    Console.WriteLine($"Error saat menerima trap: {ex.Message}")
                End Try
            End While
        End Sub
    End Module
End Namespace