using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PwfAuthCSharpClient
{
    /// <summary>
    /// PWF Auth client — one method per endpoint. Plain JSON for check-key / trial /
    /// request-hwid-reset / accounts; the AES+HMAC envelope for login / heartbeat /
    /// logout; app/info returns an encrypted reply. No external NuGet packages —
    /// JSON via System.Web.Extensions' JavaScriptSerializer.
    /// Docs: https://pwfauth.com/api.php
    /// </summary>
    public class PwfAuthClient
    {
        // One HttpClient for the whole app (recommended — avoids socket exhaustion).
        private static readonly HttpClient Http = new HttpClient();
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

        private readonly string _baseUrl;
        private readonly string _appSecret;
        private readonly CryptoEnvelope _crypto;

        public PwfAuthClient(string appSecret, string baseUrl = "https://pwfauth.com")
        {
            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentException("appSecret is required.", nameof(appSecret));
            _appSecret = appSecret.Trim();
            _baseUrl = baseUrl.TrimEnd('/');
            _crypto = new CryptoEnvelope(_appSecret);
        }

        // ── Plain read-only status ─────────────────────────────────────────────
        public async Task<ApiResult> CheckKeyAsync(string licenseKey)
        {
            string body = Json.Serialize(new Dictionary<string, string> { { "license_key", (licenseKey ?? "").Trim() } });
            return ParseFlat(await PostPlainAsync("/api/auth/check-key.php", body));
        }

        // ── Encrypted session flow: login / heartbeat / logout ─────────────────
        public async Task<ApiResult> LoginAsync(string licenseKey, string hwid)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "license_key", (licenseKey ?? "").Trim() }, { "hwid", (hwid ?? "").Trim() } });
            return ParseFlat(await SendEnvelopeAsync("/api/auth/login.php", inner));
        }

        public async Task<ApiResult> HeartbeatAsync(string sessionId, string licenseKey)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "session_id", sessionId }, { "license_key", licenseKey } });
            return ParseFlat(await SendEnvelopeAsync("/api/auth/heartbeat.php", inner));
        }

        public async Task<ApiResult> LogoutAsync(string sessionId, string licenseKey)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "session_id", sessionId }, { "license_key", licenseKey } });
            return ParseFlat(await SendEnvelopeAsync("/api/auth/logout.php", inner));
        }

        // ── Plain endpoints ────────────────────────────────────────────────────
        public async Task<ApiResult> StartTrialAsync(string hwid)
        {
            string inner = Json.Serialize(new Dictionary<string, string> { { "hwid", hwid } });
            return ParseFlat(await PostPlainAsync("/api/auth/trial.php", inner));
        }

        public async Task<ApiResult> RequestHwidResetAsync(string licenseKey, string username, string reason)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "license_key", licenseKey }, { "username", username }, { "reason", reason } });
            return ParseFlat(await PostPlainAsync("/api/auth/request-hwid-reset.php", inner));
        }

        public async Task<ApiResult> AccountRegisterAsync(string username, string password, string email)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "username", username }, { "password", password }, { "email", email } });
            return ParseFlat(await PostPlainAsync("/api/auth/account-register.php", inner));
        }

        public async Task<ApiResult> AccountLoginAsync(string username, string password, string hwid)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "username", username }, { "password", password }, { "hwid", hwid } });
            return ParseFlat(await PostPlainAsync("/api/auth/account-login.php", inner));
        }

        public async Task<ApiResult> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            string inner = Json.Serialize(new Dictionary<string, string> {
                { "username", username }, { "current_password", currentPassword }, { "new_password", newPassword } });
            return ParseFlat(await PostPlainAsync("/api/auth/change-password.php", inner));
        }

        /// <summary>Public app info (name, version, download URL) — encrypted reply.</summary>
        public async Task<ApiResult> GetAppInfoAsync()
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "/api/app/info.php"))
            {
                req.Headers.Add("X-App-Secret", _appSecret);
                using (var resp = await Http.SendAsync(req).ConfigureAwait(false))
                {
                    string raw = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return ParseFlat(CryptoEnvelope.IsEnvelope(raw) ? _crypto.Decrypt(raw) : raw);
                }
            }
        }

        // ── low-level helpers ──────────────────────────────────────────────────
        private async Task<string> SendEnvelopeAsync(string path, string innerJson)
        {
            string envelope = _crypto.Encrypt(innerJson);
            using (var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl + path))
            {
                req.Headers.Add("X-App-Secret", _appSecret);
                req.Content = new StringContent(envelope, Encoding.UTF8, "application/json");
                using (var resp = await Http.SendAsync(req).ConfigureAwait(false))
                {
                    string raw = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return CryptoEnvelope.IsEnvelope(raw) ? _crypto.Decrypt(raw) : raw;
                }
            }
        }

        private async Task<string> PostPlainAsync(string path, string innerJson)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl + path))
            {
                req.Headers.Add("X-App-Secret", _appSecret);
                req.Content = new StringContent(innerJson, Encoding.UTF8, "application/json");
                using (var resp = await Http.SendAsync(req).ConfigureAwait(false))
                    return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>Flatten a JSON reply into string fields (top level + one nested-object
        /// level of scalars) so the demo can read whatever an endpoint returns.</summary>
        private static ApiResult ParseFlat(string raw)
        {
            var r = new ApiResult { Raw = raw };
            try
            {
                var root = Json.Deserialize<Dictionary<string, object>>(raw);
                if (root == null) return r;

                foreach (var kv in root)
                {
                    if (kv.Value is Dictionary<string, object> nested)
                    {
                        foreach (var c in nested)
                            if (!(c.Value is Dictionary<string, object>) && !(c.Value is ArrayList))
                                r.Fields[c.Key] = Convert.ToString(c.Value);
                    }
                    else if (!(kv.Value is ArrayList))
                    {
                        r.Fields[kv.Key] = Convert.ToString(kv.Value);
                    }
                }

                r.Success = ToBool(Get(root, "success"));
                r.Valid = ToBool(Get(root, "valid"));
                r.Message = AsString(Get(root, "message"));
                r.ErrorCode = AsString(Get(root, "error_code"));
                r.SessionId = AsString(Get(root, "session_id"));

                if (root.TryGetValue("key", out var keyObj) && keyObj is Dictionary<string, object> key)
                {
                    r.Status = AsString(Get(key, "status"));
                    r.ExpiresAt = AsString(Get(key, "expires_at"));
                }
            }
            catch { r.Message = "Unexpected response from the server."; }
            return r;
        }

        private static object Get(Dictionary<string, object> d, string k) => d.TryGetValue(k, out var v) ? v : null;
        private static string AsString(object v) => v?.ToString();
        private static bool ToBool(object v) => v is bool b && b;
    }

    /// <summary>Generic API reply: success/valid/message/error plus flattened fields.</summary>
    public class ApiResult
    {
        public bool Success;
        public bool Valid;
        public string Message;
        public string ErrorCode;
        public string Status;
        public string ExpiresAt;
        public string SessionId;
        public string Raw;
        public readonly Dictionary<string, string> Fields =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Read any flattened field by name (empty string if absent).</summary>
        public string Field(string name, string dflt = "") => Fields.ContainsKey(name) ? Fields[name] : dflt;
    }
}
