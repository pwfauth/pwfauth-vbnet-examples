using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// ═══════════════════════════════════════════════════════════════════════════
//  PWF Auth — C# / .NET Framework 4.8.1 console example (exercises every client API)
//
//  Each numbered section calls one feature of the PWF Auth API and explains it:
//    0) App info          GET  /api/app/info.php            (encrypted reply)
//    1) Check key         POST /api/auth/check-key.php       (plain)
//    2) Login             POST /api/auth/login.php           (encrypted)
//    3) Heartbeat         POST /api/auth/heartbeat.php       (encrypted)
//    4) Logout            POST /api/auth/logout.php          (encrypted)
//    5) Start trial       POST /api/auth/trial.php           (plain)
//    6) HWID reset req.   POST /api/auth/request-hwid-reset.php (plain)
//    7) Accounts          register / login / change-password (plain)
//
//  https://pwfauth.com   ·   docs: https://pwfauth.com/api.php
// ═══════════════════════════════════════════════════════════════════════════
namespace PwfAuthCSharpClient
{
    internal static class Program
    {
        // Get your App Secret from the dashboard: Applications → your app → API key.
        // NEVER commit your real secret. Prefer the PWF_APP_SECRET environment variable.
        private const string DefaultAppSecret = "";
        private const string DefaultBaseUrl = "https://pwfauth.com";

        private static int Main(string[] args)
        {
            // .NET Framework: make sure TLS 1.2 is enabled (Cloudflare rejects older).
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            return RunAsync(args).GetAwaiter().GetResult();
        }

        private static async Task<int> RunAsync(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("PWF Auth — C# / .NET Framework 4.8.1 client demo   ·   https://pwfauth.com");
            Console.WriteLine();

            string baseUrl = Environment.GetEnvironmentVariable("PWF_BASE_URL");
            if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = DefaultBaseUrl;

            string appSecret = Environment.GetEnvironmentVariable("PWF_APP_SECRET");
            if (string.IsNullOrWhiteSpace(appSecret)) appSecret = DefaultAppSecret;
            if (string.IsNullOrWhiteSpace(appSecret))
            {
                Console.WriteLine("!  Set PWF_APP_SECRET (or DefaultAppSecret in Program.cs) first.");
                return 1;
            }

            string licenseKey = args.Length > 0 ? args[0] : null;
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                Console.Write("Enter license key: ");
                licenseKey = Console.ReadLine();
            }
            if (string.IsNullOrWhiteSpace(licenseKey)) { Console.WriteLine("No key entered."); return 1; }

            var client = new PwfAuthClient(appSecret, baseUrl);
            string hwid = Hwid.GetId();

            try
            {
                // 0) APP INFO — branding + current version + download URL. Encrypted reply.
                Section("0) App info");
                var info = await client.GetAppInfoAsync();
                if (info.Success) Ok("App: " + info.Field("name", "?") + "   v" + info.Field("version", "?"));
                else Fail("App info", info.Message, info.ErrorCode);

                // 1) CHECK KEY — lightweight read-only status. No session, no HWID binding.
                Section("1) Check key");
                var chk = await client.CheckKeyAsync(licenseKey);
                if (chk.Valid) Ok("Valid — status " + chk.Status + ", expires " + (chk.ExpiresAt ?? "never"));
                else Fail("Rejected", chk.Message, chk.ErrorCode);

                // 2) LOGIN — binds this device (HWID) and opens a session. Encrypted.
                Section("2) Login");
                Detail("HWID", hwid);
                var login = await client.LoginAsync(licenseKey, hwid);
                string sessionId = "";
                if (login.Success) { sessionId = login.SessionId; Ok("Logged in — session " + sessionId); }
                else Fail("Login", login.Message, login.ErrorCode);

                // 3) HEARTBEAT + 4) LOGOUT — only if a session opened. Encrypted.
                if (!string.IsNullOrEmpty(sessionId))
                {
                    Section("3) Heartbeat");
                    var hb = await client.HeartbeatAsync(sessionId, licenseKey);
                    if (hb.Success) Ok("Session alive"); else Fail("Session ended", hb.Message, hb.ErrorCode);

                    Section("4) Logout");
                    var lo = await client.LogoutAsync(sessionId, licenseKey);
                    if (lo.Success) Ok("Logged out"); else Fail("Logout", lo.Message, lo.ErrorCode);
                }

                // 5) TRIAL — mint a free trial key for this device (if allowed).
                Section("5) Start trial");
                var tr = await client.StartTrialAsync(hwid);
                if (tr.Success) Ok("Trial issued: " + tr.Field("license_key") + "  (expires " + tr.Field("expires_at", "?") + ")");
                else Fail("Trial", tr.Message, tr.ErrorCode);

                // 6) HWID RESET REQUEST — ask an admin to unbind the device.
                Section("6) Request HWID reset");
                var rr = await client.RequestHwidResetAsync(licenseKey, "", "Demo: switched computers");
                if (rr.Success) Ok("Reset request submitted"); else Fail("HWID reset", rr.Message, rr.ErrorCode);

                // 7) END-USER ACCOUNTS — register, login, change-password.
                Section("7) Account system");
                string demoUser = "demo_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                const string pass1 = "DemoPass!123456";
                const string pass2 = "DemoPass!654321";
                Console.WriteLine("     (creates a throwaway account: " + demoUser + ")");
                var reg = await client.AccountRegisterAsync(demoUser, pass1, demoUser + "@example.com");
                if (reg.Success) Ok("Registered"); else Fail("Register", reg.Message, reg.ErrorCode);
                var al = await client.AccountLoginAsync(demoUser, pass1, hwid);
                if (al.Success) Ok("Account login ok"); else Fail("Account login", al.Message, al.ErrorCode);
                var cp = await client.ChangePasswordAsync(demoUser, pass1, pass2);
                if (cp.Success) Ok("Password changed"); else Fail("Change password", cp.Message, cp.ErrorCode);

                Console.WriteLine();
                Console.WriteLine("Done — every client feature exercised.");
                return 0;
            }
            catch (Exception ex)
            {
                Fail("Network error", ex.Message, "");
                return 3;
            }
        }

        private static void Section(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(title);
            Console.ResetColor();
        }

        private static void Ok(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [OK] " + msg);
            Console.ResetColor();
        }

        private static void Fail(string title, string message, string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!!] " + title + ": " + (string.IsNullOrEmpty(message)
                ? (string.IsNullOrEmpty(code) ? "unknown" : code) : message));
            Console.ResetColor();
        }

        private static void Detail(string label, string value)
        {
            Console.WriteLine("     " + label + ": " + value);
        }
    }
}
