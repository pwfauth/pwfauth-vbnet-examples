using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PWFAuth;

// ═══════════════════════════════════════════════════════════════════════════
//  PWF Auth — C# / .NET Framework 4.8.1 console example, built on the official
//  PWFAuth NuGet package (it targets netstandard2.0, so it runs on .NET Framework).
//
//  The same run as the VB.NET console example:
//    0) App info        GetAppInfoAsync          GET  /api/app/info.php
//    1) Check key       CheckKeyAsync            POST /api/auth/check-key.php
//    2) Login           LoginAsync               POST /api/auth/login.php
//                       (key bound to another PC? ResetHardwareIdAsync moves it here)
//    3) Heartbeat       HeartbeatAsync           POST /api/auth/heartbeat.php
//    4) Logout          LogoutAsync              POST /api/auth/logout.php
//    5) Free trial      CreateTrialAsync         POST /api/auth/trial.php
//    6) Accounts        RegisterAccountAsync, AccountLoginAsync
//                       (or RegisterAccountWithKeyAsync when the app wants a key at sign-up)
//    7) Redeem a key    RedeemKeyAsync            (needs a spare, unused key)
//    8) New password    ChangeAccountPasswordAsync
//
//  https://pwfauth.com   ·   API reference: https://pwfauth.com/api-reference
//  Package: https://www.nuget.org/packages/PWFAuth
// ═══════════════════════════════════════════════════════════════════════════
namespace PwfAuthCSharpClient
{
    internal static class Program
    {
        // Get your App Secret from the dashboard: Applications → your app → App Settings.
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
            Console.WriteLine("PWF Auth — C# / .NET Framework 4.8.1 client demo (PWFAuth package)   ·   https://pwfauth.com");
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
            licenseKey = licenseKey.Trim();

            // Optional: an UNUSED key for section 7. It is used up there (its time moves
            // onto the demo account), so never pass the key you use every day.
            string spareKey = (args.Length > 1 ? args[1] : Environment.GetEnvironmentVariable("PWF_SPARE_KEY")) ?? "";
            spareKey = spareKey.Trim();

            // One client for the whole app: it holds the session and its own HttpClient.
            using (var client = new PwfClient(new PwfClientOptions { AppSecret = appSecret.Trim(), BaseUrl = baseUrl.Trim() }))
            {
                try
                {
                    // 0) APP INFO — public branding + current version + download URL.
                    Section("0) App info");
                    PwfResponse info = await client.GetAppInfoAsync();
                    if (info.Success)
                    {
                        Ok("App: " + Str(info, "app", "name") + "   v" + Str(info, "app", "version"));
                        if (Str(info, "app", "download_url") != "") Detail("Download", Str(info, "app", "download_url"));
                    }
                    else Fail("App info", info);

                    // 1) CHECK KEY — read-only status. No session, no device seat used.
                    Section("1) Check key");
                    PwfResponse chk = await client.CheckKeyAsync(licenseKey);
                    if (chk.Success && chk.GetBoolean("valid", false))
                        Ok("Valid — status " + Str(chk, "key", "status") + ", " + ExpiryText(chk, "key"));
                    else if (chk.Success)
                        Fail("Not valid", "status " + Str(chk, "key", "status"));     // banned, paused, expired…
                    else
                        Fail("Rejected", chk);

                    // 2) LOGIN — binds the key to this PC (its hardware id) and opens a session.
                    Section("2) Login");
                    Detail("HWID", client.HardwareId);
                    PwfResponse login = await client.LoginAsync(licenseKey);
                    if (!login.Success) Fail("Login", login);

                    // The key is bound to another PC: let the customer move it here
                    // themselves. A reset unbinds every device of the key and starts the
                    // app's cooldown (12 h by default), so always ask first.
                    if (!login.Success && BoundElsewhere(login)
                        && Ask("     This key is bound to another computer. Move it to this one? [y/N] "))
                    {
                        PwfResponse moved = await client.ResetHardwareIdAsync(licenseKey, "Moved with the C# console example");
                        if (moved.Success)
                        {
                            Ok(moved.Message);
                            login = await client.LoginAsync(licenseKey);
                            if (!login.Success) Fail("Login", login);
                        }
                        else Fail("Move license", moved);
                    }

                    if (login.Success)
                    {
                        Ok("Logged in — session " + client.SessionId);
                        Detail("Expires", NotEmpty(Str(login, "user", "expires_at"), "never"));
                        Detail("Heartbeat", "every " + client.HeartbeatIntervalSeconds + "s");
                        if (Str(login, "seller", "name") != "") Detail("Sold by", Str(login, "seller", "name"));
                        string features = EnabledFeatures(login);
                        if (features != "") Detail("Features", features);

                        // 3) HEARTBEAT — one beat by hand, to show the call. A real app calls
                        //    client.StartHeartbeat() once after login instead: it beats in the
                        //    background and raises SessionEnded when the key is banned, paused,
                        //    expired or reset (the VB WinForms example shows it).
                        Section("3) Heartbeat");
                        PwfResponse hb = await client.HeartbeatAsync();
                        if (hb.Success) Ok("Session alive"); else Fail("Session ended", hb);

                        // 4) LOGOUT — ends the session. The key stays bound to this PC.
                        Section("4) Logout");
                        PwfResponse lo = await client.LogoutAsync();
                        if (lo != null && lo.Success) Ok("Logged out"); else Fail("Logout", lo);
                    }

                    // 5) FREE TRIAL — a trial key bound to this PC (if the app allows trials).
                    Section("5) Free trial");
                    PwfResponse trial = await client.CreateTrialAsync();
                    if (trial.Success) Ok("Trial key " + Str(trial, "trial_key") + "  (expires " + Str(trial, "expires_at") + ")");
                    else Fail("Trial", trial);

                    // 6) END-USER ACCOUNTS — username/password sign-in, next to license keys.
                    Section("6) Accounts");
                    string demoUser = "demo_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    const string pass1 = "DemoPass!123456";
                    const string pass2 = "DemoPass!654321";
                    Console.WriteLine("     (creates a throwaway account: " + demoUser + ")");
                    PwfResponse reg = await client.RegisterAccountAsync(demoUser, pass1, demoUser + "@example.com");
                    if (reg.Success)
                        Ok("Registered");
                    else if (reg.ErrorCode == PwfErrorCodes.KeyRequired && spareKey != "")
                    {
                        // The app turned on "Sign-up needs a license key": the new account
                        // takes the key's time and device limit, and the key is used up.
                        Detail("Note", "this app wants a license key at sign-up");
                        reg = await client.RegisterAccountWithKeyAsync(demoUser, pass1, spareKey, demoUser + "@example.com");
                        if (reg.Success)
                        {
                            Ok("Registered with key " + spareKey + " — " + NotEmpty(Str(reg, "user", "days_remaining"), "lifetime") + " days");
                            spareKey = "";
                        }
                        else Fail("Register with key", reg);
                    }
                    else
                    {
                        Fail("Register", reg);
                        if (reg.ErrorCode == PwfErrorCodes.KeyRequired)
                            Detail("Note", "this app wants a license key at sign-up — pass an unused one as the 2nd argument");
                    }

                    if (reg.Success)
                    {
                        PwfResponse acc = await client.AccountLoginAsync(demoUser, pass1);
                        if (acc.Success) Ok("Account login — session " + client.SessionId); else Fail("Account login", acc);

                        // 7) KEYS ON ACCOUNTS — a customer buys a key and adds its time to
                        //    their account. Through the signed-in session: no password needed.
                        Section("7) Redeem a key onto the account");
                        if (spareKey == "")
                        {
                            Console.WriteLine("     Skipped — pass an unused key as the 2nd argument (or PWF_SPARE_KEY).");
                            Console.WriteLine("     That key is used up: its time moves onto the demo account.");
                        }
                        else if (!acc.Success)
                            Console.WriteLine("     Skipped — the account is not signed in.");
                        else
                        {
                            PwfResponse rd = await client.RedeemKeyAsync(spareKey);
                            if (rd.Success)
                                Ok("Redeemed " + spareKey + " — +" + NotEmpty(Str(rd, "days_added"), "0") + " days, expires "
                                   + NotEmpty(Str(rd, "expires_at"), "never"));
                            else Fail("Redeem", rd);
                        }

                        // 8) CHANGE PASSWORD — also signs the account out on every device,
                        //    this one included, so it comes last.
                        Section("8) Change the account password");
                        PwfResponse cp = await client.ChangeAccountPasswordAsync(demoUser, pass1, pass2);
                        if (cp.Success) Ok("Password changed — every session of the account was signed out"); else Fail("Change password", cp);
                        if (client.IsSignedIn) await client.LogoutAsync();   // clears the local session too
                    }

                    Console.WriteLine();
                    Console.WriteLine("Done — every client feature exercised.");
                    return 0;
                }
                catch (PwfHttpException ex) when (ex.StatusCode == 401)
                {
                    Fail("Server error", "the App Secret was refused (HTTP 401) — copy it again from App Settings");
                    return 3;
                }
                catch (PwfException ex)
                {
                    // PwfHttpException: no usable reply. PwfCryptoException: an encrypted
                    // reply failed verification. PwfSecurityException: an unencrypted
                    // "success" — something other than the license server answered.
                    Fail("Server error", ex.Message);
                    return 3;
                }
                catch (HttpRequestException ex)
                {
                    // No connection at all: offline, DNS, firewall, proxy or TLS.
                    Fail("Cannot reach " + baseUrl, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    return 3;
                }
            }
        }

        // ── Reading replies ────────────────────────────────────────────────

        // A value from the reply, following nested objects: Str(r, "app", "name").
        // Returns "" when the path is missing or null.
        private static string Str(PwfResponse r, params string[] path)
        {
            if (r == null) return "";
            JsonElement el = r.Root;
            foreach (string part in path)
            {
                JsonElement child;
                if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(part, out child)) return "";
                el = child;
            }
            switch (el.ValueKind)
            {
                case JsonValueKind.String: return el.GetString();
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False: return el.GetRawText();
                default: return "";
            }
        }

        // The feature flags that are ON, e.g. "pro, export".
        private static string EnabledFeatures(PwfResponse r)
        {
            var names = new List<string>();
            JsonElement fe;
            if (r.TryGetProperty("features", out fe) && fe.ValueKind == JsonValueKind.Object)
                foreach (JsonProperty p in fe.EnumerateObject())
                    if (p.Value.ValueKind == JsonValueKind.True) names.Add(p.Name);
            return string.Join(", ", names);
        }

        // "expires 2026-11-03T…", or why there is no date yet.
        private static string ExpiryText(PwfResponse r, string obj)
        {
            string exp = Str(r, obj, "expires_at");
            if (exp != "") return "expires " + exp;
            if (Str(r, obj, "status") == "unused") return "the time starts at the first login";
            return "never expires";
        }

        private static bool BoundElsewhere(PwfResponse r)
        {
            return r.ErrorCode == PwfErrorCodes.HwidMismatch || r.ErrorCode == PwfErrorCodes.DeviceLimit;
        }

        private static string NotEmpty(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static bool Ask(string question)
        {
            Console.Write(question);
            string answer = Console.ReadLine();
            if (answer == null) { Console.WriteLine(); return false; }
            if (Console.IsInputRedirected) Console.WriteLine(answer);   // piped answers are not echoed
            answer = answer.Trim().ToLowerInvariant();
            return answer == "y" || answer == "yes";
        }

        // ── Output ─────────────────────────────────────────────────────────

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

        private static void Fail(string title, PwfResponse r)
        {
            if (r == null) { Fail(title, "no reply"); return; }
            string text = NotEmpty(r.Message, "unknown error");
            if (!string.IsNullOrEmpty(r.ErrorCode)) text += "  (" + r.ErrorCode + ")";
            Fail(title, text);
        }

        private static void Fail(string title, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!!] " + title + ": " + message);
            Console.ResetColor();
        }

        private static void Detail(string label, string value)
        {
            Console.WriteLine("     " + label + ": " + value);
        }
    }
}
