Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports PWFAuth

' ═══════════════════════════════════════════════════════════════════════════
'  PWF Auth — VB.NET Windows Forms example (API Explorer) — code-behind
'
'  Built on the official PWFAuth NuGet package. The UI lives in Form1.Designer.vb
'  (editable in the Visual Studio Windows Forms Designer); this file wires each
'  button to one PwfClient call and writes the outcome to the Activity log.
'
'  Features:
'    • Activation : check key, login, heartbeat (StartHeartbeat + SessionEnded), logout
'    • Trial/Reset: free trial, move a license to this PC (ResetHardwareIdAsync)
'    • Accounts   : register (with or without a key), login, redeem a key, change password
'    • App info   : name / version / download URL, update check
'
'  https://pwfauth.com   ·   API reference: https://pwfauth.com/api-reference
' ═══════════════════════════════════════════════════════════════════════════
Partial Public Class Form1

    ' ── Configuration ──────────────────────────────────────────────────────
    ' Get your App Secret from the dashboard: Applications → your app → App Settings.
    ' NEVER commit your real secret. Prefer the PWF_APP_SECRET environment variable.
    Private Const DefaultAppSecret As String = ""
    ' NOTE: VB is case-insensitive — this must NOT be named "BaseUrl" or it would
    ' collide with a local "baseUrl" and self-assign to Nothing.
    Private Const DefaultBaseUrl As String = "https://pwfauth.com"
    ' The version of THIS app, sent by "Check for an update".
    Private Const AppVersion As String = "1.0.0"

    ' ── Status colors (semantic — used by the handlers, not the layout) ─────
    Private Shared ReadOnly TextC As Color = Color.FromArgb(33, 37, 41)
    Private Shared ReadOnly GreenC As Color = Color.FromArgb(25, 135, 84)
    Private Shared ReadOnly RedC As Color = Color.FromArgb(220, 53, 69)
    Private Shared ReadOnly MutedC As Color = Color.FromArgb(108, 117, 125)

    ' ── One client for the whole app ────────────────────────────────────────
    ' PwfClient holds the session (SessionId, LicenseKey) and runs the heartbeat,
    ' so keep one instance. It is rebuilt only when the App Secret or Base URL
    ' fields change.
    Private _client As PwfClient
    Private _clientSecret As String = ""
    Private _clientUrl As String = ""

    Public Sub New()
        InitializeComponent()          ' from Form1.Designer.vb
        InitRuntime()
        ActiveControl = txtKey         ' start in the License key box
    End Sub

    ' Fill the settings fields from the environment (falling back to the consts),
    ' show the device HWID, and greet the log.
    Private Sub InitRuntime()
        Dim envSecret = Environment.GetEnvironmentVariable("PWF_APP_SECRET")
        txtSecret.Text = If(String.IsNullOrWhiteSpace(envSecret), DefaultAppSecret, envSecret)
        Dim envUrl = Environment.GetEnvironmentVariable("PWF_BASE_URL")
        txtBaseUrl.Text = If(String.IsNullOrWhiteSpace(envUrl), DefaultBaseUrl, envUrl)
        Dim hwid = HardwareId.Get()    ' the id PwfClient binds licenses to
        lblHwid.Text = "HWID  " & hwid
        LogLine("Ready. Device HWID = " & hwid)
    End Sub

    ' ── Convenience: select-all when the key box gains focus ────────────────
    Private Sub txtKey_Enter(sender As Object, e As EventArgs) Handles txtKey.Enter
        txtKey.SelectAll()
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Activation
    ' ═════════════════════════════════════════════════════════════════════════

    ' CHECK KEY — read-only status; no session, no device seat used.
    Private Async Sub OnCheckClick(sender As Object, e As EventArgs) Handles btnCheck.Click
        Dim key = txtKey.Text.Trim()
        If key = "" Then LogLine("!  Enter a license key first.") : Return
        Await RunAsync(sender, "check-key",
            Async Function()
                Dim r = Await GetClient().CheckKeyAsync(key)
                If r.Success AndAlso r.GetBoolean("valid", False) Then
                    Dim extra = ""
                    If Str(r, "key", "days_remaining") <> "" Then extra = " · " & Str(r, "key", "days_remaining") & " days left"
                    Dim features = EnabledFeatures(r)
                    If features <> "" Then extra &= " · features: " & features
                    LogLine("[OK] check-key: valid — status " & Str(r, "key", "status") & ", expires " &
                            NotEmpty(Str(r, "key", "expires_at"), "at first login + duration") & extra)
                ElseIf r.Success Then
                    LogLine("[--] check-key: not valid — status " & Str(r, "key", "status"))
                Else
                    LogLine("[--] check-key: " & Reason(r))
                End If
            End Function)
    End Sub

    ' LOGIN — binds the key to this PC and opens a session, then starts the
    ' heartbeat. StartHeartbeat is called here, on the UI thread, so SessionEnded
    ' is raised on the UI thread too and may touch controls directly.
    Private Async Sub OnLoginClick(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim key = txtKey.Text.Trim()
        If key = "" Then LogLine("!  Enter a license key first.") : Return
        Await RunAsync(sender, "login",
            Async Function()
                Dim client = GetClient()
                If client.IsSignedIn Then Await client.LogoutAsync()   ' one session per client
                Dim r = Await client.LoginAsync(key)
                If r.Success Then
                    client.StartHeartbeat()
                    Dim features = EnabledFeatures(r)
                    lblSession.ForeColor = GreenC
                    lblSession.Text = "Session " & client.SessionId & Environment.NewLine &
                                      "Expires " & NotEmpty(Str(r, "user", "expires_at"), "never") &
                                      If(Str(r, "seller", "name") <> "", "  ·  sold by " & Str(r, "seller", "name"), "") & Environment.NewLine &
                                      "Heartbeat running every " & client.HeartbeatIntervalSeconds & "s" &
                                      If(features <> "", Environment.NewLine & "Features: " & features, "")
                    LogLine("[OK] login: session " & client.SessionId & " · heartbeat every " & client.HeartbeatIntervalSeconds & "s")
                Else
                    lblSession.ForeColor = RedC
                    lblSession.Text = "Login refused: " & Reason(r)
                    LogLine("[--] login: " & Reason(r))
                    If r.ErrorCode = PwfErrorCodes.HwidMismatch OrElse r.ErrorCode = PwfErrorCodes.DeviceLimit Then
                        ' Bound to another PC: the customer can move it here themselves.
                        txtResetKey.Text = key
                        LogLine("   This key is bound to another computer — Trial & Reset → ""Move license to this PC"".")
                    End If
                End If
            End Function)
    End Sub

    ' ONE HEARTBEAT by hand. The background heartbeat started at login already
    ' does this every few seconds; this button just shows a single call.
    Private Async Sub OnHeartbeatClick(sender As Object, e As EventArgs) Handles btnHeartbeat.Click
        If _client Is Nothing OrElse Not _client.IsSignedIn Then LogLine("!  Login first — no active session.") : Return
        Await RunAsync(sender, "heartbeat",
            Async Function()
                Dim r = Await _client.HeartbeatAsync()
                If r.Success Then LogLine("[OK] heartbeat: session alive") Else LogLine("[--] heartbeat: " & Reason(r))
            End Function)
    End Sub

    ' LOGOUT — stops the heartbeat and ends the session. The key stays bound here.
    Private Async Sub OnLogoutClick(sender As Object, e As EventArgs) Handles btnLogout.Click
        If _client Is Nothing OrElse Not _client.IsSignedIn Then LogLine("!  No active session to log out.") : Return
        Await RunAsync(sender, "logout",
            Async Function()
                Dim r = Await _client.LogoutAsync()
                If r IsNot Nothing AndAlso r.Success Then LogLine("[OK] logout: session closed") Else LogLine("[--] logout: " & Reason(r))
                ShowNoSession()
            End Function)
    End Sub

    ' The heartbeat ended the session: banned, paused, expired, reset, revoked,
    ' maintenance, a password change — or the server could not be reached for
    ' several beats in a row. Raised on the UI thread (see OnLoginClick).
    Private Sub Client_SessionEnded(sender As Object, e As SessionEndedEventArgs)
        lblSession.ForeColor = RedC
        lblSession.Text = "Session ended: " & e.Message & "  (" & e.ErrorCode & ")"
        LogLine("[--] session ended by the server: " & e.Message & "  (" & e.ErrorCode & ")")
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Trial & Reset
    ' ═════════════════════════════════════════════════════════════════════════

    ' FREE TRIAL — a trial key bound to this PC (if the app allows trials).
    Private Async Sub OnTrialClick(sender As Object, e As EventArgs) Handles btnTrial.Click
        Await RunAsync(sender, "trial",
            Async Function()
                Dim r = Await GetClient().CreateTrialAsync()
                If r.Success Then
                    LogLine("[OK] trial: key " & Str(r, "trial_key") & " (expires " & Str(r, "expires_at") & ")")
                    txtKey.Text = Str(r, "trial_key")
                Else
                    LogLine("[--] trial: " & Reason(r))
                End If
            End Function)
    End Sub

    ' MOVE A LICENSE TO THIS PC — unbinds the key from every computer it is bound
    ' to, so the next login binds it here. The app's cooldown applies (12 h by
    ' default), so a real app asks the customer first.
    Private Async Sub OnResetClick(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim key = txtResetKey.Text.Trim()
        If key = "" Then LogLine("!  Enter the license key to move.") : Return
        If MessageBox.Show(Me, "Move " & key & " to this computer?" & Environment.NewLine &
                           "It stops working on the computer it is bound to now.",
                           "Move license", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Await RunAsync(sender, "move-license",
            Async Function()
                Dim r = Await GetClient().ResetHardwareIdAsync(key, txtResetReason.Text.Trim())
                If r.Success Then
                    LogLine("[OK] move-license: " & Reason(r) & " Next move allowed: " & NotEmpty(Str(r, "next_reset_at"), "any time"))
                    txtKey.Text = key
                Else
                    LogLine("[--] move-license: " & Reason(r))
                End If
            End Function)
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Accounts
    ' ═════════════════════════════════════════════════════════════════════════

    ' REGISTER — create an end-user account (username / password).
    Private Async Sub OnRegisterClick(sender As Object, e As EventArgs) Handles btnRegister.Click
        EnsureDemoUser()
        Await RunAsync(sender, "register",
            Async Function()
                Dim r = Await GetClient().RegisterAccountAsync(txtAccUser.Text.Trim(), txtAccPass.Text, txtAccEmail.Text.Trim())
                If r.Success Then
                    LogLine("[OK] register: account '" & txtAccUser.Text.Trim() & "' created")
                ElseIf r.ErrorCode = PwfErrorCodes.KeyRequired Then
                    LogLine("[--] register: " & Reason(r) & " — enter an unused key and use ""Register with the key"".")
                Else
                    LogLine("[--] register: " & Reason(r))
                End If
            End Function)
    End Sub

    ' REGISTER WITH A KEY — the account takes the key's time and device limit,
    ' and the key is used up. Needed when the app turned on "Sign-up needs a
    ' license key" in App Settings.
    Private Async Sub OnRegisterKeyClick(sender As Object, e As EventArgs) Handles btnRegisterKey.Click
        If txtAccKey.Text.Trim() = "" Then LogLine("!  Enter an unused license key first.") : Return
        EnsureDemoUser()
        Await RunAsync(sender, "register-with-key",
            Async Function()
                Dim r = Await GetClient().RegisterAccountWithKeyAsync(txtAccUser.Text.Trim(), txtAccPass.Text, txtAccKey.Text.Trim(), txtAccEmail.Text.Trim())
                If r.Success Then
                    LogLine("[OK] register-with-key: account '" & txtAccUser.Text.Trim() & "' created, expires " &
                            NotEmpty(Str(r, "user", "expires_at"), "never"))
                Else
                    LogLine("[--] register-with-key: " & Reason(r))
                End If
            End Function)
    End Sub

    ' ACCOUNT LOGIN — signs the account in on this PC and starts the heartbeat,
    ' exactly like a license login.
    Private Async Sub OnAccLoginClick(sender As Object, e As EventArgs) Handles btnAccLogin.Click
        If txtAccUser.Text.Trim() = "" Then LogLine("!  Enter (or register) a username first.") : Return
        Await RunAsync(sender, "account-login",
            Async Function()
                Dim client = GetClient()
                If client.IsSignedIn Then Await client.LogoutAsync()   ' one session per client
                Dim r = Await client.AccountLoginAsync(txtAccUser.Text.Trim(), txtAccPass.Text)
                If r.Success Then
                    client.StartHeartbeat()
                    lblSession.ForeColor = GreenC
                    lblSession.Text = "Account " & txtAccUser.Text.Trim() & " · session " & client.SessionId & Environment.NewLine &
                                      "Expires " & NotEmpty(Str(r, "user", "expires_at"), "never") & Environment.NewLine &
                                      "Heartbeat running every " & client.HeartbeatIntervalSeconds & "s"
                    LogLine("[OK] account-login: '" & txtAccUser.Text.Trim() & "' signed in, expires " & NotEmpty(Str(r, "user", "expires_at"), "never"))
                Else
                    LogLine("[--] account-login: " & Reason(r))
                End If
            End Function)
    End Sub

    ' REDEEM A KEY — adds an unused key's time to the account and uses the key up.
    ' Works with the username and password, even while the account has expired.
    Private Async Sub OnRedeemClick(sender As Object, e As EventArgs) Handles btnRedeem.Click
        If txtAccUser.Text.Trim() = "" Then LogLine("!  Enter the account username first.") : Return
        If txtAccKey.Text.Trim() = "" Then LogLine("!  Enter an unused license key first.") : Return
        Await RunAsync(sender, "redeem",
            Async Function()
                Dim r = Await GetClient().RedeemKeyAsync(txtAccUser.Text.Trim(), txtAccPass.Text, txtAccKey.Text.Trim())
                If r.Success Then
                    LogLine("[OK] redeem: +" & NotEmpty(Str(r, "days_added"), "0") & " days, expires " & NotEmpty(Str(r, "expires_at"), "never"))
                Else
                    LogLine("[--] redeem: " & Reason(r))
                End If
            End Function)
    End Sub

    ' CHANGE PASSWORD — also signs the account out on every device: a running
    ' heartbeat for it reports that through SessionEnded within one beat.
    Private Async Sub OnChangePassClick(sender As Object, e As EventArgs) Handles btnChangePass.Click
        If txtAccUser.Text.Trim() = "" Then LogLine("!  Enter the account username first.") : Return
        Await RunAsync(sender, "change-password",
            Async Function()
                Dim r = Await GetClient().ChangeAccountPasswordAsync(txtAccUser.Text.Trim(), txtCurPass.Text, txtNewPass.Text)
                If r.Success Then
                    LogLine("[OK] change-password: password updated — every session of the account was signed out")
                    txtAccPass.Text = txtNewPass.Text
                Else
                    LogLine("[--] change-password: " & Reason(r))
                End If
            End Function)
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  App info
    ' ═════════════════════════════════════════════════════════════════════════

    ' APP INFO — public branding + version + download URL. Encrypted reply.
    Private Async Sub OnAppInfoClick(sender As Object, e As EventArgs) Handles btnAppInfo.Click
        Await RunAsync(sender, "app-info",
            Async Function()
                Dim r = Await GetClient().GetAppInfoAsync()
                If r.Success Then
                    lblAppInfo.ForeColor = TextC
                    lblAppInfo.Text = "Name:       " & Str(r, "app", "name") & Environment.NewLine &
                                      "Version:    " & Str(r, "app", "version") & Environment.NewLine &
                                      "Download:   " & NotEmpty(Str(r, "app", "download_url"), "—") & Environment.NewLine &
                                      "Message:    " & NotEmpty(Str(r, "app", "login_message"), "—") & Environment.NewLine &
                                      "Maintenance: " & If(Str(r, "app", "maintenance_mode") = "true", "ON — " & Str(r, "app", "maintenance_msg"), "off")
                    LogLine("[OK] app-info: " & Str(r, "app", "name") & " v" & Str(r, "app", "version"))
                Else
                    lblAppInfo.ForeColor = RedC
                    lblAppInfo.Text = "Failed: " & Reason(r)
                    LogLine("[--] app-info: " & Reason(r))
                End If
            End Function)
    End Sub

    ' UPDATE CHECK — is there a build newer than this app's version?
    Private Async Sub OnUpdateClick(sender As Object, e As EventArgs) Handles btnUpdate.Click
        Await RunAsync(sender, "update-check",
            Async Function()
                Dim r = Await GetClient().CheckUpdateAsync(AppVersion)
                If Not r.Success Then
                    LogLine("[--] update-check: " & Reason(r))
                ElseIf r.GetBoolean("update_available", False) Then
                    LogLine("[OK] update-check: v" & Str(r, "update", "version") & " is available (this app is v" & AppVersion & ")")
                Else
                    LogLine("[OK] update-check: v" & AppVersion & " is the latest version")
                End If
            End Function)
    End Sub

    ' ═════════════════════════════════════════════════════════════════════════
    '  Helpers
    ' ═════════════════════════════════════════════════════════════════════════

    ' The client for the current settings; rebuilt when the App Secret or Base URL
    ' changed. PwfClientOptions.Validate throws for an empty secret or a bad URL.
    Private Function GetClient() As PwfClient
        Dim secret = txtSecret.Text.Trim()
        If secret = "" Then secret = DefaultAppSecret
        Dim url = txtBaseUrl.Text.Trim()
        If url = "" Then url = DefaultBaseUrl
        If _client IsNot Nothing AndAlso secret = _clientSecret AndAlso url = _clientUrl Then Return _client

        If _client IsNot Nothing Then
            RemoveHandler _client.SessionEnded, AddressOf Client_SessionEnded
            _client.Dispose()          ' stops its heartbeat; the server ends the session on timeout
            _client = Nothing
            ShowNoSession()
            LogLine("Settings changed — started a new client.")
        End If
        Dim created As New PwfClient(New PwfClientOptions With {.AppSecret = secret, .BaseUrl = url})
        AddHandler created.SessionEnded, AddressOf Client_SessionEnded
        _client = created
        _clientSecret = secret
        _clientUrl = url
        Return created
    End Function

    ' Runs one button's work: disables the button meanwhile and logs any error.
    Private Async Function RunAsync(sender As Object, name As String, work As Func(Of Task)) As Task
        Dim b = DirectCast(sender, Button)
        b.Enabled = False
        Try
            Await work()
        Catch ex As PwfHttpException When ex.StatusCode = 401
            LogLine("[!!] " & name & ": the App Secret was refused (HTTP 401) — copy it again from App Settings.")
        Catch ex As PwfException
            ' No usable reply, a reply that failed verification, or an unencrypted
            ' "success" that did not come from the license server.
            LogLine("[!!] " & name & ": " & ex.Message)
        Catch ex As HttpRequestException
            LogLine("[!!] " & name & ": cannot reach the server — " & ex.Message)
        Catch ex As ArgumentException
            LogLine("[!!] " & name & ": " & ex.Message)   ' e.g. an empty App Secret
        Finally
            b.Enabled = True
        End Try
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

    Private Sub ShowNoSession()
        lblSession.ForeColor = MutedC
        lblSession.Text = "No session — Check a key, then Login to open one."
    End Sub

    ' A value from the reply, following nested objects: Str(r, "app", "name").
    Private Shared Function Str(r As PwfResponse, ParamArray path As String()) As String
        If r Is Nothing Then Return ""
        Dim el As JsonElement = r.Root
        For Each part As String In path
            Dim child As JsonElement
            If el.ValueKind <> JsonValueKind.Object OrElse Not el.TryGetProperty(part, child) Then Return ""
            el = child
        Next
        Select Case el.ValueKind
            Case JsonValueKind.String : Return el.GetString()
            Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False : Return el.GetRawText()
            Case Else : Return ""
        End Select
    End Function

    ' The feature flags that are ON, e.g. "pro, export".
    Private Shared Function EnabledFeatures(r As PwfResponse) As String
        Dim names As New List(Of String)
        Dim fe As JsonElement
        If r.TryGetProperty("features", fe) AndAlso fe.ValueKind = JsonValueKind.Object Then
            For Each p In fe.EnumerateObject()
                If p.Value.ValueKind = JsonValueKind.True Then names.Add(p.Name)
            Next
        End If
        Return String.Join(", ", names)
    End Function

    Private Shared Function NotEmpty(value As String, fallback As String) As String
        Return If(String.IsNullOrEmpty(value), fallback, value)
    End Function

    ' The reply's message (safe to show the user) plus its error code.
    Private Shared Function Reason(r As PwfResponse) As String
        If r Is Nothing Then Return "no reply"
        Dim text = NotEmpty(r.Message, "unknown error")
        If Not String.IsNullOrEmpty(r.ErrorCode) Then text &= "  (" & r.ErrorCode & ")"
        Return text
    End Function

    Private Sub LogLine(msg As String)
        txtLog.AppendText("[" & DateTime.Now.ToString("HH:mm:ss") & "] " & msg & Environment.NewLine)
    End Sub

    ' Closing the window: end the session now instead of letting the server time
    ' it out. Task.Run keeps the wait off the UI thread's context.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If _client IsNot Nothing Then
            Dim client = _client
            _client = Nothing
            RemoveHandler client.SessionEnded, AddressOf Client_SessionEnded
            Try
                If client.IsSignedIn Then Task.Run(Function() client.LogoutAsync()).Wait(TimeSpan.FromSeconds(3))
            Catch ex As AggregateException
                ' Offline: the server ends the session on its own timeout.
            End Try
            client.Dispose()
        End If
    End Sub
End Class
