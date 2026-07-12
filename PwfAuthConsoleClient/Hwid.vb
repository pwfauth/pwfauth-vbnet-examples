Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' A stable per-machine hardware id sent at login (the server binds the license
''' to it). This demo derives it from machine + user + OS — good enough to show
''' the flow. A production app would combine more hardware signals (disk / CPU /
''' MAC); see HWIDHelper.vb in the full PWFLoginVB SDK for a richer version.
''' </summary>
Public Module Hwid
    Public Function GetId() As String
        Dim seed = String.Join("|", {
            Environment.MachineName,
            Environment.UserName,
            Environment.OSVersion.Platform.ToString(),
            Environment.ProcessorCount.ToString()
        })
        Dim hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed))
        Return Convert.ToHexString(hash).Substring(0, 32).ToLowerInvariant()
    End Function
End Module
