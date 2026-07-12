Imports System
Imports System.Text
Imports System.Threading.Tasks

' ═══════════════════════════════════════════════════════════════════════════
'  PWF Auth — VB.NET console example (exercises every client API)
'
'  Each numbered section calls one feature of the PWF Auth API and explains it:
'    0) App info          GET  /api/app/info.php            (encrypted reply)
'    1) Check key         POST /api/auth/check-key.php       (plain)
'    2) Login             POST /api/auth/login.php           (encrypted)
'    3) Heartbeat         POST /api/auth/heartbeat.php       (encrypted)
'    4) Logout            POST /api/auth/logout.php          (encrypted)
'    5) Start trial       POST /api/auth/trial.php           (plain)
'    6) HWID reset req.   POST /api/auth/request-hwid-reset.php (plain)
'    7) Accounts          register / login / change-password (plain)
'
'  https://pwfauth.com   ·   docs: https://pwfauth.com/api.php
' ═══════════════════════════════════════════════════════════════════════════
Module Program

    ' ── Configuration ──────────────────────────────────────────────────────
    ' Get your App Secret from the dashboard: Applications → your app → API key.
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
        Console.WriteLine("PWF Auth — VB.NET client demo   ·   https://pwfauth.com")
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

        Dim client As New PwfAuthClient(appSecret, baseUrl)
        Dim deviceHwid = Hwid.GetId()

        Try
            ' 0) APP INFO — public branding + current version + download URL + changelog.
            '    A launcher shows this on its login screen and compares "version" to
            '    offer an OTA update from "download_url". The reply is AES-encrypted.
            Section("0) App info")
            Dim info = Await client.GetAppInfoAsync()
            If info.Success Then
                Ok("App: " & info.Field("name", "?") & "   v" & info.Field("version", "?"))
                If info.Field("download_url") <> "" Then Detail("Download", info.Field("download_url"))
            Else
                Fail("App info", info.Message, info.ErrorCode)
            End If

            ' 1) CHECK KEY — lightweight read-only status. No session, no HWID binding.
            Section("1) Check key")
            Dim chk = Await client.CheckKeyAsync(licenseKey)
            If chk.Valid Then
                Ok("Valid — status " & chk.Status & ", expires " & If(chk.ExpiresAt, "never"))
            Else
                Fail("Rejected", chk.Message, chk.ErrorCode)
            End If

            ' 2) LOGIN — binds this device (HWID) and opens a heartbeat session. Encrypted.
            Section("2) Login")
            Detail("HWID", deviceHwid)
            Dim login = Await client.LoginAsync(licenseKey, deviceHwid)
            Dim sessionId = ""
            If login.Success Then
                sessionId = login.SessionId
                Ok("Logged in — session " & sessionId)
                Detail("Heartbeat", "every " & login.HeartbeatInterval & "s")
                If login.Features.Count > 0 Then Detail("Features", String.Join(", ", login.Features))
            Else
                Fail("Login", login.Message, login.ErrorCode)
            End If

            ' 3) HEARTBEAT — keep the session alive; the server can return a kill code. Encrypted.
            If sessionId <> "" Then
                Section("3) Heartbeat")
                Dim hb = Await client.HeartbeatAsync(sessionId, licenseKey)
                If hb.Success Then Ok("Session alive") Else Fail("Session ended", hb.Message, hb.ErrorCode)

                ' 4) LOGOUT — end the session. Encrypted.
                Section("4) Logout")
                Dim lo = Await client.LogoutAsync(sessionId, licenseKey)
                If lo.Success Then Ok("Logged out") Else Fail("Logout", lo.Message, lo.ErrorCode)
            End If

            ' 5) TRIAL — mint a free trial key for this device (if the app allows trials).
            Section("5) Start trial")
            Dim tr = Await client.StartTrialAsync(deviceHwid)
            If tr.Success Then
                Ok("Trial issued: " & tr.Field("license_key") & "  (expires " & tr.Field("expires_at", "?") & ")")
            Else
                Fail("Trial", tr.Message, tr.ErrorCode)
            End If

            ' 6) HWID RESET REQUEST — the user asks an admin to unbind their device.
            Section("6) Request HWID reset")
            Dim rr = Await client.RequestHwidResetAsync(licenseKey, "", "Demo: switched computers")
            If rr.Success Then Ok("Reset request submitted") Else Fail("HWID reset", rr.Message, rr.ErrorCode)

            ' 7) END-USER ACCOUNTS — username/password auth, separate from license keys.
            '    Registers a throwaway account, logs in, then changes its password.
            Section("7) Account system")
            Dim demoUser = "demo_" & DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            Const pass1 = "DemoPass!123456"
            Const pass2 = "DemoPass!654321"
            Console.WriteLine("     (creates a throwaway account: " & demoUser & ")")
            Dim reg = Await client.AccountRegisterAsync(demoUser, pass1, demoUser & "@example.com")
            If reg.Success Then Ok("Registered") Else Fail("Register", reg.Message, reg.ErrorCode)
            Dim al = Await client.AccountLoginAsync(demoUser, pass1, deviceHwid)
            If al.Success Then Ok("Account login ok") Else Fail("Account login", al.Message, al.ErrorCode)
            Dim cp = Await client.ChangePasswordAsync(demoUser, pass1, pass2)
            If cp.Success Then Ok("Password changed") Else Fail("Change password", cp.Message, cp.ErrorCode)

            Console.WriteLine()
            Console.WriteLine("Done — every client feature exercised.")
            Return 0
        Catch ex As Exception
            Fail("Network error", ex.Message, "")
            Return 3
        End Try
    End Function

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

    Private Sub Fail(title As String, message As String, code As String)
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("  [!!] " & title & ": " & If(String.IsNullOrEmpty(message), If(code, "unknown"), message))
        Console.ResetColor()
    End Sub

    Private Sub Detail(label As String, value As String)
        Console.WriteLine("     " & label & ": " & value)
    End Sub
End Module
