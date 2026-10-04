Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports PWFAuth

' ═══════════════════════════════════════════════════════════════════════════
'  PWF Auth — VB.NET console example, built on the official PWFAuth NuGet package
'
'  Each numbered section calls one feature of the PWF Auth API and explains it:
'    0) App info        GetAppInfoAsync          GET  /api/app/info.php
'    1) Check key       CheckKeyAsync            POST /api/auth/check-key.php
'    2) Login           LoginAsync               POST /api/auth/login.php
'                       (key bound to another PC? ResetHardwareIdAsync moves it here)
'    3) Heartbeat       HeartbeatAsync           POST /api/auth/heartbeat.php
'    4) Logout          LogoutAsync              POST /api/auth/logout.php
'    5) Free trial      CreateTrialAsync         POST /api/auth/trial.php
'    6) Accounts        RegisterAccountAsync, AccountLoginAsync
'                       (or RegisterAccountWithKeyAsync when the app wants a key at sign-up)
'    7) Redeem a key    RedeemKeyAsync            (needs a spare, unused key)
'    8) New password    ChangeAccountPasswordAsync
'
'  The package encrypts the requests that must be encrypted, repairs a wrong PC
'  clock by itself and binds licenses to this machine's hardware id.
'
'  https://pwfauth.com   ·   API reference: https://pwfauth.com/api-reference
'  Package: https://www.nuget.org/packages/PWFAuth
' ═══════════════════════════════════════════════════════════════════════════
Module Program

    ' ── Configuration ──────────────────────────────────────────────────────
    ' Get your App Secret from the dashboard: Applications → your app → App Settings.
    ' NEVER commit your real secret. Prefer the PWF_APP_SECRET environment variable.
    Private Const DefaultAppSecret As String = ""
    ' NOTE: VB is case-insensitive, so this const must NOT be named "BaseUrl" —
    ' it would collide with the local "baseUrl" below and self-assign to Nothing.
    Private Const DefaultBaseUrl As String = "https://pwfauth.com"

    Sub Main(args As String())
        Environment.ExitCode = RunAsync(args).GetAwaiter().GetResult()
    End Sub

    Private Async Function RunAsync(args As String()) As Task(Of Integer)
        Console.OutputEncoding = Encoding.UTF8
        Console.WriteLine("PWF Auth — VB.NET client demo (PWFAuth package)   ·   https://pwfauth.com")
        Console.WriteLine()

        Dim baseUrl = Environment.GetEnvironmentVariable("PWF_BASE_URL")
        If String.IsNullOrWhiteSpace(baseUrl) Then baseUrl = DefaultBaseUrl

        Dim appSecret = Environment.GetEnvironmentVariable("PWF_APP_SECRET")
        If String.IsNullOrWhiteSpace(appSecret) Then appSecret = DefaultAppSecret
        If String.IsNullOrWhiteSpace(appSecret) OrElse appSecret = "YOUR_APP_SECRET" Then
            Console.WriteLine("!  Set PWF_APP_SECRET (or DefaultAppSecret in Program.vb) first.")
            Return 1
        End If

        Dim licenseKey = If(args.Length > 0, args(0), Nothing)
        If String.IsNullOrWhiteSpace(licenseKey) Then
            Console.Write("Enter license key: ")
            licenseKey = Console.ReadLine()
        End If
        If String.IsNullOrWhiteSpace(licenseKey) Then
            Console.WriteLine("No key entered.")
            Return 1
        End If
        licenseKey = licenseKey.Trim()

        ' Optional: an UNUSED key for section 7. It is used up there (its time moves
        ' onto the demo account), so never pass the key you use every day.
        Dim spareKey = If(args.Length > 1, args(1), Environment.GetEnvironmentVariable("PWF_SPARE_KEY"))
        spareKey = If(spareKey, "").Trim()

        ' One client for the whole app: it holds the session (SessionId, LicenseKey)
        ' and its own HttpClient.
        Using client As New PwfClient(New PwfClientOptions With {.AppSecret = appSecret.Trim(), .BaseUrl = baseUrl.Trim()})
            Try
                ' 0) APP INFO — public branding + current version + download URL.
                '    A launcher shows this on its login screen and compares "version"
                '    to offer an update. The reply is AES-encrypted.
                Section("0) App info")
                Dim info = Await client.GetAppInfoAsync()
                If info.Success Then
                    Ok("App: " & Str(info, "app", "name") & "   v" & Str(info, "app", "version"))
                    If Str(info, "app", "download_url") <> "" Then Detail("Download", Str(info, "app", "download_url"))
                Else
                    Fail("App info", info)
                End If

                ' 1) CHECK KEY — read-only status. No session, no device seat used.
                Section("1) Check key")
                Dim chk = Await client.CheckKeyAsync(licenseKey)
                If chk.Success AndAlso chk.GetBoolean("valid", False) Then
                    Ok("Valid — status " & Str(chk, "key", "status") & ", " & ExpiryText(chk, "key"))
                ElseIf chk.Success Then
                    Fail("Not valid", "status " & Str(chk, "key", "status"))     ' banned, paused, expired…
                Else
                    Fail("Rejected", chk)
                End If

                ' 2) LOGIN — binds the key to this PC (its hardware id) and opens a session.
                Section("2) Login")
                Detail("HWID", client.HardwareId)
                Dim login = Await client.LoginAsync(licenseKey)
                If Not login.Success Then Fail("Login", login)

                ' The key is bound to another PC: let the customer move it here
                ' themselves. A reset unbinds every device of the key and starts the
                ' app's cooldown (12 h by default), so always ask first.
                If Not login.Success AndAlso BoundElsewhere(login) AndAlso
                   Ask("     This key is bound to another computer. Move it to this one? [y/N] ") Then
                    Dim moved = Await client.ResetHardwareIdAsync(licenseKey, "Moved with the VB.NET console example")
                    If moved.Success Then
                        Ok(moved.Message)
                        login = Await client.LoginAsync(licenseKey)
                        If Not login.Success Then Fail("Login", login)
                    Else
                        Fail("Move license", moved)
                    End If
                End If

                If login.Success Then
                    Ok("Logged in — session " & client.SessionId)
                    Detail("Expires", NotEmpty(Str(login, "user", "expires_at"), "never"))
                    Detail("Heartbeat", "every " & client.HeartbeatIntervalSeconds & "s")
                    If Str(login, "seller", "name") <> "" Then Detail("Sold by", Str(login, "seller", "name"))
                    Dim features = EnabledFeatures(login)
                    If features <> "" Then Detail("Features", features)

                    ' 3) HEARTBEAT — one beat by hand, to show the call. A real app calls
                    '    client.StartHeartbeat() once after login instead: it beats in the
                    '    background and raises SessionEnded when the key is banned, paused,
                    '    expired or reset (the WinForms example shows it).
                    Section("3) Heartbeat")
                    Dim hb = Await client.HeartbeatAsync()
                    If hb.Success Then Ok("Session alive") Else Fail("Session ended", hb)

                    ' 4) LOGOUT — ends the session. The key stays bound to this PC.
                    Section("4) Logout")
                    Dim lo = Await client.LogoutAsync()
                    If lo IsNot Nothing AndAlso lo.Success Then Ok("Logged out") Else Fail("Logout", lo)
                End If

                ' 5) FREE TRIAL — a trial key bound to this PC (if the app allows trials).
                Section("5) Free trial")
                Dim trial = Await client.CreateTrialAsync()
                If trial.Success Then
                    Ok("Trial key " & Str(trial, "trial_key") & "  (expires " & Str(trial, "expires_at") & ")")
                Else
                    Fail("Trial", trial)
                End If

                ' 6) END-USER ACCOUNTS — username/password sign-in, next to license keys.
                '    Registers a throwaway account, signs in, then changes its password.
                Section("6) Accounts")
                Dim demoUser = "demo_" & DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                Const pass1 = "DemoPass!123456"
                Const pass2 = "DemoPass!654321"
                Console.WriteLine("     (creates a throwaway account: " & demoUser & ")")
                Dim reg = Await client.RegisterAccountAsync(demoUser, pass1, demoUser & "@example.com")
                If reg.Success Then
                    Ok("Registered")
                ElseIf reg.ErrorCode = PwfErrorCodes.KeyRequired AndAlso spareKey <> "" Then
                    ' The app turned on "Sign-up needs a license key": the new account
                    ' takes the key's time and device limit, and the key is used up.
                    Detail("Note", "this app wants a license key at sign-up")
                    reg = Await client.RegisterAccountWithKeyAsync(demoUser, pass1, spareKey, demoUser & "@example.com")
                    If reg.Success Then
                        Ok("Registered with key " & spareKey & " — " & NotEmpty(Str(reg, "user", "days_remaining"), "lifetime") & " days")
                        spareKey = ""
                    Else
                        Fail("Register with key", reg)
                    End If
                Else
                    Fail("Register", reg)
                    If reg.ErrorCode = PwfErrorCodes.KeyRequired Then
                        Detail("Note", "this app wants a license key at sign-up — pass an unused one as the 2nd argument")
                    End If
                End If

                If reg.Success Then
                    Dim acc = Await client.AccountLoginAsync(demoUser, pass1)
                    If acc.Success Then Ok("Account login — session " & client.SessionId) Else Fail("Account login", acc)

                    ' 7) KEYS ON ACCOUNTS — a customer buys a key and adds its time to
                    '    their account. Through the signed-in session: no password needed.
                    Section("7) Redeem a key onto the account")
                    If spareKey = "" Then
                        Console.WriteLine("     Skipped — pass an unused key as the 2nd argument (or PWF_SPARE_KEY).")
                        Console.WriteLine("     That key is used up: its time moves onto the demo account.")
                    ElseIf Not acc.Success Then
                        Console.WriteLine("     Skipped — the account is not signed in.")
                    Else
                        Dim rd = Await client.RedeemKeyAsync(spareKey)
                        If rd.Success Then
                            Ok("Redeemed " & spareKey & " — +" & NotEmpty(Str(rd, "days_added"), "0") & " days, " &
                               "expires " & NotEmpty(Str(rd, "expires_at"), "never"))
                        Else
                            Fail("Redeem", rd)
                        End If
                    End If

                    ' 8) CHANGE PASSWORD — also signs the account out on every device,
                    '    this one included, so it comes last.
                    Section("8) Change the account password")
                    Dim cp = Await client.ChangeAccountPasswordAsync(demoUser, pass1, pass2)
                    If cp.Success Then Ok("Password changed — every session of the account was signed out") Else Fail("Change password", cp)
                    If client.IsSignedIn Then Await client.LogoutAsync()   ' clears the local session too
                End If

                Console.WriteLine()
                Console.WriteLine("Done — every client feature exercised.")
                Return 0
            Catch ex As PwfHttpException When ex.StatusCode = 401
                Fail("Server error", "the App Secret was refused (HTTP 401) — copy it again from App Settings")
                Return 3
            Catch ex As PwfException
                ' PwfHttpException: no usable reply. PwfCryptoException: an encrypted
                ' reply failed verification. PwfSecurityException: an unencrypted
                ' "success" — something other than the license server answered.
                Fail("Server error", ex.Message)
                Return 3
            Catch ex As HttpRequestException
                ' No connection at all: offline, DNS, firewall or proxy.
                Fail("Cannot reach " & baseUrl, ex.Message)
                Return 3
            End Try
        End Using
    End Function

    ' ── Reading replies ────────────────────────────────────────────────────

    ' A value from the reply, following nested objects: Str(r, "app", "name").
    ' Returns "" when the path is missing or null.
    Private Function Str(r As PwfResponse, ParamArray path As String()) As String
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

    ' The feature flags that are ON, e.g. "pro, export". The server sends an object
    ' of name → true/false.
    Private Function EnabledFeatures(r As PwfResponse) As String
        Dim names As New List(Of String)
        Dim fe As JsonElement
        If r.TryGetProperty("features", fe) AndAlso fe.ValueKind = JsonValueKind.Object Then
            For Each p In fe.EnumerateObject()
                If p.Value.ValueKind = JsonValueKind.True Then names.Add(p.Name)
            Next
        End If
        Return String.Join(", ", names)
    End Function

    ' "expires 2026-11-03T…", or why there is no date yet.
    Private Function ExpiryText(r As PwfResponse, obj As String) As String
        Dim exp = Str(r, obj, "expires_at")
        If exp <> "" Then Return "expires " & exp
        If Str(r, obj, "status") = "unused" Then Return "the time starts at the first login"
        Return "never expires"
    End Function

    Private Function BoundElsewhere(r As PwfResponse) As Boolean
        Return r.ErrorCode = PwfErrorCodes.HwidMismatch OrElse r.ErrorCode = PwfErrorCodes.DeviceLimit
    End Function

    Private Function NotEmpty(value As String, fallback As String) As String
        Return If(String.IsNullOrEmpty(value), fallback, value)
    End Function

    Private Function Ask(question As String) As Boolean
        Console.Write(question)
        Dim answer = Console.ReadLine()
        If answer Is Nothing Then Console.WriteLine() : Return False
        If Console.IsInputRedirected Then Console.WriteLine(answer)   ' piped answers are not echoed
        answer = answer.Trim().ToLowerInvariant()
        Return answer = "y" OrElse answer = "yes"
    End Function

    ' ── Output ─────────────────────────────────────────────────────────────

    Private Sub Section(title As String)
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine(title)
        Console.ResetColor()
    End Sub

    Private Sub Ok(msg As String)
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("  [OK] " & msg)
        Console.ResetColor()
    End Sub

    Private Sub Fail(title As String, r As PwfResponse)
        If r Is Nothing Then Fail(title, "no reply") : Return
        Dim text = NotEmpty(r.Message, "unknown error")
        If Not String.IsNullOrEmpty(r.ErrorCode) Then text &= "  (" & r.ErrorCode & ")"
        Fail(title, text)
    End Sub

    Private Sub Fail(title As String, message As String)
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("  [!!] " & title & ": " & message)
        Console.ResetColor()
    End Sub

    Private Sub Detail(label As String, value As String)
        Console.WriteLine("     " & label & ": " & value)
    End Sub
End Module
