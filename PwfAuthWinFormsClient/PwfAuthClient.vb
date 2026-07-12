Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks

''' <summary>
''' Minimal PWF Auth license client.
''' Calls POST /api/auth/check-key.php with your app secret in the X-App-Secret
''' header and a plain-JSON body { "license_key": "..." }, then parses the reply.
''' Docs: https://pwfauth.com/api.php
''' </summary>
Public Class PwfAuthClient

    ' One HttpClient for the whole app (recommended — avoids socket exhaustion).
    Private Shared ReadOnly Http As New HttpClient()

    Private ReadOnly _baseUrl As String
    Private ReadOnly _appSecret As String
    Private ReadOnly _crypto As CryptoEnvelope

    ''' <param name="appSecret">Your app's secret from the PWF Auth dashboard.</param>
    ''' <param name="baseUrl">API base URL (default https://pwfauth.com).</param>
    Public Sub New(appSecret As String, Optional baseUrl As String = "https://pwfauth.com")
        If String.IsNullOrWhiteSpace(appSecret) Then
            Throw New ArgumentException("appSecret is required.", NameOf(appSecret))
        End If
        _appSecret = appSecret.Trim()
        _baseUrl = baseUrl.TrimEnd("/"c)
        _crypto = New CryptoEnvelope(_appSecret)
    End Sub

    ''' <summary>
    ''' Validate a license key. A normal "invalid key" is NOT an error — it comes
    ''' back as a result with Valid = False. Only transport failures throw.
    ''' </summary>
    Public Async Function CheckKeyAsync(licenseKey As String) As Task(Of KeyCheckResult)
        Dim body = New Dictionary(Of String, String) From {
            {"license_key", If(licenseKey, "").Trim()}
        }
        Dim json = JsonSerializer.Serialize(body)

        Using req As New HttpRequestMessage(HttpMethod.Post, _baseUrl & "/api/auth/check-key.php")
            req.Headers.Add("X-App-Secret", _appSecret)
            req.Content = New StringContent(json, Encoding.UTF8, "application/json")

            Using resp = Await Http.SendAsync(req).ConfigureAwait(False)
                Dim raw = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Return Parse(raw)
            End Using
        End Using
    End Function

    Private Shared Function Parse(raw As String) As KeyCheckResult
        Dim result As New KeyCheckResult With {.Raw = raw}
        Try
            Using doc = JsonDocument.Parse(raw)
                Dim root = doc.RootElement
                result.Success = GetBool(root, "success")
                result.Valid = GetBool(root, "valid")
                result.Message = GetStr(root, "message")
                result.ErrorCode = GetStr(root, "error_code")

                Dim keyEl As JsonElement
                If root.TryGetProperty("key", keyEl) AndAlso keyEl.ValueKind = JsonValueKind.Object Then
                    result.Status = GetStr(keyEl, "status")
                    result.ExpiresAt = GetStr(keyEl, "expires_at")
                    Dim daysEl As JsonElement
                    If keyEl.TryGetProperty("days_remaining", daysEl) AndAlso daysEl.ValueKind = JsonValueKind.Number Then
                        result.DaysRemaining = daysEl.GetInt32()
                    End If
                End If

                Dim featEl As JsonElement
                If root.TryGetProperty("features", featEl) AndAlso featEl.ValueKind = JsonValueKind.Array Then
                    For Each f In featEl.EnumerateArray()
                        If f.ValueKind = JsonValueKind.String Then result.Features.Add(f.GetString())
                    Next
                End If
            End Using
        Catch ex As JsonException
            result.Success = False
            result.Message = "Unexpected response from the server."
        End Try
        Return result
    End Function

    Private Shared Function GetBool(el As JsonElement, name As String) As Boolean
        Dim v As JsonElement
        Return el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.True
    End Function

    Private Shared Function GetStr(el As JsonElement, name As String) As String
        Dim v As JsonElement
        If el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then Return v.GetString()
        Return Nothing
    End Function

    ' ── Encrypted session flow: login / heartbeat / logout ──────────────────
    ' These endpoints require the AES-256 + HMAC envelope (unlike check-key).

    ''' <summary>Activate a key on this device: binds the HWID and opens a session.
    ''' Returns the session id, heartbeat interval, and features.</summary>
    Public Async Function LoginAsync(licenseKey As String, hwid As String) As Task(Of SessionResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"license_key", If(licenseKey, "").Trim()}, {"hwid", If(hwid, "").Trim()}
        })
        Return ParseSession(Await SendEnvelopeAsync("/api/auth/login.php", inner))
    End Function

    ''' <summary>Keep the session alive. If the server killed it (revoke/ban/etc.),
    ''' Success is False and Message/ErrorCode explain why.</summary>
    Public Async Function HeartbeatAsync(sessionId As String, licenseKey As String) As Task(Of SessionResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"session_id", sessionId}, {"license_key", licenseKey}
        })
        Return ParseSession(Await SendEnvelopeAsync("/api/auth/heartbeat.php", inner))
    End Function

    ''' <summary>End the session.</summary>
    Public Async Function LogoutAsync(sessionId As String, licenseKey As String) As Task(Of SessionResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"session_id", sessionId}, {"license_key", licenseKey}
        })
        Return ParseSession(Await SendEnvelopeAsync("/api/auth/logout.php", inner))
    End Function

    ''' <summary>POST an encrypted envelope, then return the DECRYPTED inner JSON —
    ''' or, if the server rejected the request before crypto ran, its plain JSON.</summary>
    Private Async Function SendEnvelopeAsync(path As String, innerJson As String) As Task(Of String)
        Dim envelope = _crypto.Encrypt(innerJson)
        Using req As New HttpRequestMessage(HttpMethod.Post, _baseUrl & path)
            req.Headers.Add("X-App-Secret", _appSecret)
            req.Content = New StringContent(envelope, Encoding.UTF8, "application/json")
            Using resp = Await Http.SendAsync(req).ConfigureAwait(False)
                Dim raw = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Return If(CryptoEnvelope.IsEnvelope(raw), _crypto.Decrypt(raw), raw)
            End Using
        End Using
    End Function

    Private Shared Function ParseSession(inner As String) As SessionResult
        Dim r As New SessionResult With {.Raw = inner}
        Try
            Using doc = JsonDocument.Parse(inner)
                Dim root = doc.RootElement
                r.Success = GetBool(root, "success")
                r.Message = GetStr(root, "message")
                r.ErrorCode = GetStr(root, "error_code")
                r.SessionId = GetStr(root, "session_id")
                Dim hb As JsonElement
                If root.TryGetProperty("heartbeat_interval", hb) AndAlso hb.ValueKind = JsonValueKind.Number Then
                    r.HeartbeatInterval = hb.GetInt32()
                End If
                Dim fe As JsonElement
                If root.TryGetProperty("features", fe) Then
                    If fe.ValueKind = JsonValueKind.Array Then
                        For Each f In fe.EnumerateArray()
                            If f.ValueKind = JsonValueKind.String Then r.Features.Add(f.GetString())
                        Next
                    ElseIf fe.ValueKind = JsonValueKind.Object Then
                        For Each prop In fe.EnumerateObject()
                            r.Features.Add(prop.Name)
                        Next
                    End If
                End If
            End Using
        Catch ex As JsonException
            r.Success = False
            r.Message = "Unexpected response from the server."
        End Try
        Return r
    End Function

    ' ── Additional features (plain JSON, except app-info's encrypted reply) ─────

    ''' <summary>Start a free trial on this device — the server mints a trial key bound to the HWID.</summary>
    Public Async Function StartTrialAsync(hwid As String) As Task(Of ApiResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {{"hwid", hwid}})
        Return ParseFlat(Await PostPlainAsync("/api/auth/trial.php", inner))
    End Function

    ''' <summary>Ask an admin to clear the device (HWID) binding on a license.</summary>
    Public Async Function RequestHwidResetAsync(licenseKey As String, username As String, reason As String) As Task(Of ApiResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"license_key", licenseKey}, {"username", username}, {"reason", reason}})
        Return ParseFlat(Await PostPlainAsync("/api/auth/request-hwid-reset.php", inner))
    End Function

    ''' <summary>Create an end-user account (username / password) for this app.</summary>
    Public Async Function AccountRegisterAsync(username As String, password As String, email As String) As Task(Of ApiResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"username", username}, {"password", password}, {"email", email}})
        Return ParseFlat(Await PostPlainAsync("/api/auth/account-register.php", inner))
    End Function

    ''' <summary>Log in with an end-user account; opens a session bound to the HWID.</summary>
    Public Async Function AccountLoginAsync(username As String, password As String, hwid As String) As Task(Of ApiResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"username", username}, {"password", password}, {"hwid", hwid}})
        Return ParseFlat(Await PostPlainAsync("/api/auth/account-login.php", inner))
    End Function

    ''' <summary>Change an end-user account's password.</summary>
    Public Async Function ChangePasswordAsync(username As String, currentPassword As String, newPassword As String) As Task(Of ApiResult)
        Dim inner = JsonSerializer.Serialize(New Dictionary(Of String, String) From {
            {"username", username}, {"current_password", currentPassword}, {"new_password", newPassword}})
        Return ParseFlat(Await PostPlainAsync("/api/auth/change-password.php", inner))
    End Function

    ''' <summary>Public app info: name, version, download URL, changelog, socials
    ''' (used for branding + OTA update checks). The reply is encrypted.</summary>
    Public Async Function GetAppInfoAsync() As Task(Of ApiResult)
        Using req As New HttpRequestMessage(HttpMethod.Get, _baseUrl & "/api/app/info.php")
            req.Headers.Add("X-App-Secret", _appSecret)
            Using resp = Await Http.SendAsync(req).ConfigureAwait(False)
                Dim raw = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                Return ParseFlat(If(CryptoEnvelope.IsEnvelope(raw), _crypto.Decrypt(raw), raw))
            End Using
        End Using
    End Function

    ' ── low-level helpers ──

    Private Async Function PostPlainAsync(path As String, innerJson As String) As Task(Of String)
        Using req As New HttpRequestMessage(HttpMethod.Post, _baseUrl & path)
            req.Headers.Add("X-App-Secret", _appSecret)
            req.Content = New StringContent(innerJson, Encoding.UTF8, "application/json")
            Using resp = Await Http.SendAsync(req).ConfigureAwait(False)
                Return Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
            End Using
        End Using
    End Function

    ''' <summary>Flatten a JSON reply into string fields (top level + one nested-object level),
    ''' so the demo can print whatever an endpoint returns without a bespoke class each time.</summary>
    Private Shared Function ParseFlat(inner As String) As ApiResult
        Dim r As New ApiResult With {.Raw = inner}
        Try
            Using doc = JsonDocument.Parse(inner)
                Dim root = doc.RootElement
                If root.ValueKind = JsonValueKind.Object Then
                    For Each prop In root.EnumerateObject()
                        Select Case prop.Value.ValueKind
                            Case JsonValueKind.String
                                r.Fields(prop.Name) = prop.Value.GetString()
                            Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False
                                r.Fields(prop.Name) = prop.Value.GetRawText()
                            Case JsonValueKind.Object
                                For Each child In prop.Value.EnumerateObject()
                                    Select Case child.Value.ValueKind
                                        Case JsonValueKind.String
                                            r.Fields(child.Name) = child.Value.GetString()
                                        Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False
                                            r.Fields(child.Name) = child.Value.GetRawText()
                                    End Select
                                Next
                        End Select
                    Next
                    r.Success = GetBool(root, "success")
                    r.Message = GetStr(root, "message")
                    r.ErrorCode = GetStr(root, "error_code")
                End If
            End Using
        Catch ex As JsonException
            r.Message = "Unexpected response from the server."
        End Try
        Return r
    End Function
End Class

''' <summary>Result of a license-key check.</summary>
Public Class KeyCheckResult
    Public Property Success As Boolean
    Public Property Valid As Boolean
    Public Property Status As String
    Public Property ExpiresAt As String
    Public Property DaysRemaining As Integer?
    Public Property Message As String
    Public Property ErrorCode As String
    Public Property Features As New List(Of String)
    Public Property Raw As String
End Class

''' <summary>Result of a login / heartbeat / logout call.</summary>
Public Class SessionResult
    Public Property Success As Boolean
    Public Property Message As String
    Public Property ErrorCode As String
    Public Property SessionId As String
    Public Property HeartbeatInterval As Integer = 30
    Public Property Features As New List(Of String)
    Public Property Raw As String
End Class

''' <summary>Generic API reply: success/message/error plus flattened string fields.</summary>
Public Class ApiResult
    Public Property Success As Boolean
    Public Property Message As String
    Public Property ErrorCode As String
    Public ReadOnly Property Fields As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Public Property Raw As String

    ''' <summary>Read any flattened field by name (empty string if absent).</summary>
    Public Function Field(name As String, Optional dflt As String = "") As String
        Return If(Fields.ContainsKey(name), Fields(name), dflt)
    End Function
End Class
