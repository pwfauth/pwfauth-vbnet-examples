Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json

''' <summary>
''' AES-256-CBC + HMAC-SHA256 envelope encryption — the wire format the PWF Auth
''' server uses for login / heartbeat / logout. Mirrors PayloadCrypto.php exactly.
'''
''' Envelope:  { "p": base64(IV(16) || ciphertext), "t": unix_ts,
'''             "s": hex(hmac_sha256(p + t, macKey)) }
''' Keys:      encKey = SHA256("enc:" + appSecret),  macKey = SHA256("mac:" + appSecret)
''' </summary>
Public Class CryptoEnvelope

    Private ReadOnly _encKey As Byte()
    Private ReadOnly _macKey As Byte()
    Private Const MaxDriftSeconds As Long = 300   ' keep in sync with the server

    Public Sub New(appSecret As String)
        If String.IsNullOrEmpty(appSecret) Then Throw New ArgumentException("App secret cannot be empty.")
        _encKey = SHA256.HashData(Encoding.UTF8.GetBytes("enc:" & appSecret))
        _macKey = SHA256.HashData(Encoding.UTF8.GetBytes("mac:" & appSecret))
    End Sub

    ''' <summary>Wrap a plain JSON string in an encrypted {p,t,s} envelope.</summary>
    Public Function Encrypt(innerJson As String) As String
        Dim iv(15) As Byte
        RandomNumberGenerator.Fill(iv)

        Dim cipher As Byte()
        Using alg = Aes.Create()
            alg.Key = _encKey
            cipher = alg.EncryptCbc(Encoding.UTF8.GetBytes(innerJson), iv, PaddingMode.PKCS7)
        End Using

        Dim combined(iv.Length + cipher.Length - 1) As Byte
        Array.Copy(iv, 0, combined, 0, iv.Length)
        Array.Copy(cipher, 0, combined, iv.Length, cipher.Length)

        Dim p = Convert.ToBase64String(combined)
        Dim t = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        Dim s = HmacHex(p & t.ToString())

        Return JsonSerializer.Serialize(New Dictionary(Of String, Object) From {
            {"p", p}, {"t", t}, {"s", s}
        })
    End Function

    ''' <summary>Verify + decrypt a {p,t,s} envelope; returns the inner JSON string.</summary>
    Public Function Decrypt(envelopeJson As String) As String
        Using doc = JsonDocument.Parse(envelopeJson)
            Dim root = doc.RootElement
            Dim p = root.GetProperty("p").GetString()
            Dim t = root.GetProperty("t").GetInt64()
            Dim s = root.GetProperty("s").GetString()

            Dim expected = HmacHex(p & t.ToString())
            If Not CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expected),
                    Encoding.ASCII.GetBytes(If(s, "").ToLowerInvariant())) Then
                Throw New InvalidOperationException("HMAC verification failed")
            End If

            If Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - t) > MaxDriftSeconds Then
                Throw New InvalidOperationException("Request expired (replay protection)")
            End If

            Dim combined = Convert.FromBase64String(p)
            If combined.Length <= 16 Then Throw New InvalidOperationException("Malformed ciphertext")

            Dim iv(15) As Byte
            Array.Copy(combined, 0, iv, 0, 16)
            Dim cipher(combined.Length - 17) As Byte
            Array.Copy(combined, 16, cipher, 0, combined.Length - 16)

            Using alg = Aes.Create()
                alg.Key = _encKey
                Return Encoding.UTF8.GetString(alg.DecryptCbc(cipher, iv, PaddingMode.PKCS7))
            End Using
        End Using
    End Function

    ''' <summary>True when the string looks like an encrypted {p,t,s} envelope
    ''' (vs a plain-JSON error the server returns before crypto runs).</summary>
    Public Shared Function IsEnvelope(json As String) As Boolean
        Try
            Using doc = JsonDocument.Parse(json)
                Dim root = doc.RootElement
                Dim tmp As JsonElement
                Return root.ValueKind = JsonValueKind.Object AndAlso
                       root.TryGetProperty("p", tmp) AndAlso
                       root.TryGetProperty("t", tmp) AndAlso
                       root.TryGetProperty("s", tmp)
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function HmacHex(message As String) As String
        Dim hash = HMACSHA256.HashData(_macKey, Encoding.UTF8.GetBytes(message))
        Return Convert.ToHexString(hash).ToLowerInvariant()
    End Function
End Class
