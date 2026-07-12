Imports System
Imports System.Drawing
Imports System.Windows.Forms

' ═══════════════════════════════════════════════════════════════════════════
'  PWF Auth — VB.NET Windows Forms example (API Explorer) — code-behind
'
'  The UI itself lives in Form1.Designer.vb (editable in the Visual Studio
'  Windows Forms Designer). This file only wires each control's event to the
'  matching PwfAuthClient call and writes the outcome to the Activity log.
'
'  Endpoints exercised:
'    • Activation : check-key, login (HWID bind), heartbeat, logout   (session)
'    • Trial/Reset: start a free trial, request an HWID reset
'    • Accounts   : register, login, change-password  (end-user accounts)
'    • App info   : name / version / download URL      (OTA update check)
'
'  https://pwfauth.com   ·   docs: https://pwfauth.com/api.php
' ═══════════════════════════════════════════════════════════════════════════
Partial Public Class Form1

    ' ── Configuration ──────────────────────────────────────────────────────
    ' Get your App Secret from the dashboard: Applications → your app → API key.
    ' NEVER commit your real secret. Prefer the PWF_APP_SECRET environment variable.
    Private Const DefaultAppSecret As String = ""
    ' NOTE: VB is case-insensitive — this must NOT be named "BaseUrl" or it would
    ' collide with a local "baseUrl" and self-assign to Nothing.
    Private Const DefaultBaseUrl As String = "https://pwfauth.com"

    ' ── Status colors (semantic — used by the handlers, not the layout) ─────
    Private Shared ReadOnly TextC As Color = Color.FromArgb(33, 37, 41)
    Private Shared ReadOnly GreenC As Color = Color.FromArgb(25, 135, 84)
    Private Shared ReadOnly RedC As Color = Color.FromArgb(220, 53, 69)
    Private Shared ReadOnly MutedC As Color = Color.FromArgb(108, 117, 125)

    ' ── Session state (set by login, used by heartbeat/logout) ──────────────
    Private _sessionId As String = ""
    Private _activeKey As String = ""
    Private _deviceHwid As String = ""

    Public Sub New()
        InitializeComponent()          ' from Form1.Designer.vb
        _deviceHwid = Hwid.GetId()     ' stable per-machine id sent at login
        InitRuntime()
    End Sub

    ' Fill the settings fields from the environment (falling back to the consts),
    ' show the device HWID, and greet the log. Env reading belongs here, not in
    ' the designer.
    Private Sub InitRuntime()
        Dim envSecret = Environment.GetEnvironmentVariable("PWF_APP_SECRET")
        txtSecret.Text = If(String.IsNullOrWhiteSpace(envSecret), DefaultAppSecret, envSecret)
        Dim envUrl = Environment.GetEnvironmentVariable("PWF_BASE_URL")
        txtBaseUrl.Text = If(String.IsNullOrWhiteSpace(envUrl), DefaultBaseUrl, envUrl)
        lblHwid.Text = "HWID  " & _deviceHwid
        LogLine("Ready. Device HWID = " & _deviceHwid)
    End Sub

    ' ── Convenience: select-all when the key box gains focus ────────────────
    Private Sub txtKey_Enter(sender As Object, e As EventArgs) Handles txtKey.Enter
        txtKey.SelectAll()
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Event handlers — one per feature. Each builds a client from the current
    '  settings, calls the API, and writes the outcome to the Activity log.
    ' ═════════════════════════════════════════════════════════════════════════

    ' 1) CHECK KEY — lightweight read-only status; no session, no HWID binding.
    Private Async Sub OnCheckClick(sender As Object, e As EventArgs) Handles btnCheck.Click
        Dim key = txtKey.Text.Trim()
        If key = "" Then LogLine("!  Enter a license key first.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().CheckKeyAsync(key)
            If r.Valid Then
                Dim extra = ""
                If r.DaysRemaining.HasValue Then extra = " · " & r.DaysRemaining.Value.ToString() & " days left"
                If r.Features.Count > 0 Then extra &= " · features: " & String.Join(", ", r.Features)
                LogLine("[OK] check-key: valid — status " & r.Status & ", expires " & If(r.ExpiresAt, "never") & extra)
            Else
                LogLine("[--] check-key: " & Reason(r.Message, r.ErrorCode))
            End If
        Catch ex As Exception
            LogLine("[!!] check-key error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 2) LOGIN — binds this device (HWID) and opens a heartbeat session. Encrypted.
    Private Async Sub OnLoginClick(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim key = txtKey.Text.Trim()
        If key = "" Then LogLine("!  Enter a license key first.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().LoginAsync(key, _deviceHwid)
            If r.Success Then
                _sessionId = r.SessionId
                _activeKey = key
                Dim feat = If(r.Features.Count > 0, "  features: " & String.Join(", ", r.Features), "")
                lblSession.ForeColor = GreenC
                lblSession.Text = "Session " & r.SessionId & Environment.NewLine &
                                  "Heartbeat every " & r.HeartbeatInterval & "s" & Environment.NewLine & feat.Trim()
                LogLine("[OK] login: session " & r.SessionId & " · heartbeat " & r.HeartbeatInterval & "s (encrypted)")
            Else
                lblSession.ForeColor = RedC
                lblSession.Text = "Login rejected: " & Reason(r.Message, r.ErrorCode)
                LogLine("[--] login: " & Reason(r.Message, r.ErrorCode))
            End If
        Catch ex As Exception
            LogLine("[!!] login error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 3) HEARTBEAT — keep the session alive; the server can return a kill code. Encrypted.
    Private Async Sub OnHeartbeatClick(sender As Object, e As EventArgs) Handles btnHeartbeat.Click
        If _sessionId = "" Then LogLine("!  Login first — no active session.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().HeartbeatAsync(_sessionId, _activeKey)
            If r.Success Then
                LogLine("[OK] heartbeat: session alive (encrypted)")
            Else
                LogLine("[--] heartbeat: " & Reason(r.Message, r.ErrorCode) & "  (session ended)")
                _sessionId = "" : _activeKey = ""
                lblSession.ForeColor = RedC
                lblSession.Text = "Session ended: " & Reason(r.Message, r.ErrorCode)
            End If
        Catch ex As Exception
            LogLine("[!!] heartbeat error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 4) LOGOUT — end the session. Encrypted.
    Private Async Sub OnLogoutClick(sender As Object, e As EventArgs) Handles btnLogout.Click
        If _sessionId = "" Then LogLine("!  No active session to log out.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().LogoutAsync(_sessionId, _activeKey)
            If r.Success Then LogLine("[OK] logout: session closed") Else LogLine("[--] logout: " & Reason(r.Message, r.ErrorCode))
        Catch ex As Exception
            LogLine("[!!] logout error: " & ex.Message)
        Finally
            _sessionId = "" : _activeKey = ""
            lblSession.ForeColor = MutedC
            lblSession.Text = "No session — Check a key, then Login to open one."
            b.Enabled = True
        End Try
    End Sub

    ' 5) TRIAL — mint a free trial key for this device (if the app allows trials).
    Private Async Sub OnTrialClick(sender As Object, e As EventArgs) Handles btnTrial.Click
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().StartTrialAsync(_deviceHwid)
            If r.Success Then
                LogLine("[OK] trial: issued " & r.Field("license_key") & " (expires " & r.Field("expires_at", "?") & ")")
            Else
                LogLine("[--] trial: " & Reason(r.Message, r.ErrorCode))
            End If
        Catch ex As Exception
            LogLine("[!!] trial error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 6) HWID RESET REQUEST — the user asks an admin to unbind their device.
    Private Async Sub OnResetClick(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim key = txtResetKey.Text.Trim()
        If key = "" Then LogLine("!  Enter the license key to reset.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().RequestHwidResetAsync(key, "", txtResetReason.Text.Trim())
            If r.Success Then LogLine("[OK] hwid-reset: " & Reason(r.Message, "request submitted")) Else LogLine("[--] hwid-reset: " & Reason(r.Message, r.ErrorCode))
        Catch ex As Exception
            LogLine("[!!] hwid-reset error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 7a) REGISTER — create an end-user account (username / password).
    Private Async Sub OnRegisterClick(sender As Object, e As EventArgs) Handles btnRegister.Click
        EnsureDemoUser()
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().AccountRegisterAsync(txtAccUser.Text.Trim(), txtAccPass.Text, txtAccEmail.Text.Trim())
            If r.Success Then LogLine("[OK] register: account '" & txtAccUser.Text.Trim() & "' created") Else LogLine("[--] register: " & Reason(r.Message, r.ErrorCode))
        Catch ex As Exception
            LogLine("[!!] register error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 7b) ACCOUNT LOGIN — log in with the end-user account; binds the HWID.
    Private Async Sub OnAccLoginClick(sender As Object, e As EventArgs) Handles btnAccLogin.Click
        If txtAccUser.Text.Trim() = "" Then LogLine("!  Enter (or register) a username first.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().AccountLoginAsync(txtAccUser.Text.Trim(), txtAccPass.Text, _deviceHwid)
            If r.Success Then LogLine("[OK] account-login: '" & txtAccUser.Text.Trim() & "' authenticated") Else LogLine("[--] account-login: " & Reason(r.Message, r.ErrorCode))
        Catch ex As Exception
            LogLine("[!!] account-login error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 7c) CHANGE PASSWORD — change the end-user account's password.
    Private Async Sub OnChangePassClick(sender As Object, e As EventArgs) Handles btnChangePass.Click
        If txtAccUser.Text.Trim() = "" Then LogLine("!  Enter the account username first.") : Return
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().ChangePasswordAsync(txtAccUser.Text.Trim(), txtCurPass.Text, txtNewPass.Text)
            If r.Success Then LogLine("[OK] change-password: password updated") Else LogLine("[--] change-password: " & Reason(r.Message, r.ErrorCode))
        Catch ex As Exception
            LogLine("[!!] change-password error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' 0) APP INFO — public branding + version + download URL. Encrypted reply.
    Private Async Sub OnAppInfoClick(sender As Object, e As EventArgs) Handles btnAppInfo.Click
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Dim r = Await GetClient().GetAppInfoAsync()
            If r.Success Then
                lblAppInfo.ForeColor = TextC
                lblAppInfo.Text = "Name:      " & r.Field("name", "?") & Environment.NewLine &
                                  "Version:   " & r.Field("version", "?") & Environment.NewLine &
                                  "Download:  " & r.Field("download_url", "—") & Environment.NewLine &
                                  "Changelog: " & r.Field("changelog", "—")
                LogLine("[OK] app-info: " & r.Field("name", "?") & " v" & r.Field("version", "?") & " (encrypted reply)")
            Else
                lblAppInfo.ForeColor = RedC
                lblAppInfo.Text = "Failed: " & Reason(r.Message, r.ErrorCode)
                LogLine("[--] app-info: " & Reason(r.Message, r.ErrorCode))
            End If
        Catch ex As Exception
            LogLine("[!!] app-info error: " & ex.Message)
        Finally
            b.Enabled = True
        End Try
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Helpers
    ' ═════════════════════════════════════════════════════════════════════════

    ' Build a client from whatever is currently in the settings fields.
    Private Function GetClient() As PwfAuthClient
        Dim secret = txtSecret.Text.Trim()
        If secret = "" Then secret = DefaultAppSecret
        Dim url = txtBaseUrl.Text.Trim()
        If url = "" Then url = DefaultBaseUrl
        Return New PwfAuthClient(secret, url)
    End Function

    ' Auto-fill a throwaway account when the user left the fields blank, so the
    ' demo works with a single click (and never collides with an existing user).
    Private Sub EnsureDemoUser()
        If txtAccUser.Text.Trim() = "" Then txtAccUser.Text = "demo_" & DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
        If txtAccPass.Text = "" Then txtAccPass.Text = "DemoPass!123456"
        If txtAccEmail.Text.Trim() = "" Then txtAccEmail.Text = txtAccUser.Text.Trim() & "@example.com"
        If txtCurPass.Text = "" Then txtCurPass.Text = txtAccPass.Text
        If txtNewPass.Text = "" Then txtNewPass.Text = "DemoPass!654321"
    End Sub

    Private Sub LogLine(msg As String)
        txtLog.AppendText("[" & DateTime.Now.ToString("HH:mm:ss") & "] " & msg & Environment.NewLine)
    End Sub

    Private Shared Function Reason(msg As String, fallback As String) As String
        If Not String.IsNullOrEmpty(msg) Then Return msg
        If Not String.IsNullOrEmpty(fallback) Then Return fallback
        Return "unknown"
    End Function
End Class
