Option Explicit On
Module Module_ini
    Private Declare Ansi Function GetPrivateProfileString Lib "kernel32.dll" Alias "GetPrivateProfileStringA" (ByVal IpApplicationName As String, ByVal IpKeyName As String, ByVal IpDefault As String, ByVal IpReturnedString As String, ByVal nSize As Integer, ByVal IpFileName As String) As Integer
    Private Declare Ansi Function WritePrivateProfileString Lib "kernel32.dll" Alias "WritePrivateProfileStringA" (ByVal IpApplicationName As String, ByVal IpKeyName As String, ByVal IpString As String, ByVal IpFileName As String) As Integer

    'fungsi untuk membaca file *.ini
    Public Function GetIniSetting(ByVal strINIFile As String, ByVal strSection As String, ByVal strKey As String) As String
        Dim strValue As String = ""
        Try
            strValue = Space(1024)
            GetPrivateProfileString(strSection, strKey, "NOT_FOUND", strValue, 1024, strINIFile)
            Do While InStrRev(strValue, " ") = Len(strValue)
                strValue = Mid(strValue, 1, Len(strValue) - 1)
            Loop
            'remove a special chr in the last place
            strValue = Mid(strValue, 1, Len(strValue) - 1)
            GetIniSetting = strValue
        Catch ex As Exception
            If Err.Number <> 0 Then Err.Raise(Err.Number, , "Error from Functions.SetIniSetting " & Err.Description)
        End Try
        Return strValue
    End Function

    'fungsi untuk menulis file *.ini
    Public Sub SetIniSettings(ByVal strINIFile As String, ByVal strSection As String, ByVal strKey As String, ByVal strValue As String)
        Try
            WritePrivateProfileString(strSection, strKey, strValue, strINIFile)
        Catch ex As Exception
            If Err.Number <> 0 Then Err.Raise(Err.Number, , "Error from Functions.SetIniSetting " & Err.Description)
        End Try
    End Sub
End Module
