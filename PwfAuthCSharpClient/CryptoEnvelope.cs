using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace PwfAuthCSharpClient
{
    /// <summary>
    /// AES-256-CBC + HMAC-SHA256 envelope encryption — the wire format the PWF Auth
    /// server uses for login / heartbeat / logout. Mirrors PayloadCrypto.php exactly.
    ///
    /// Written for .NET Framework 4.8.1, so it uses the classic crypto APIs
    /// (CreateEncryptor / ComputeHash) instead of the .NET-5+ one-shots
    /// (EncryptCbc / SHA256.HashData / Convert.ToHexString) used by the VB net8 port.
    ///
    /// Envelope:  { "p": base64(IV(16) || ciphertext), "t": unix_ts,
    ///             "s": hex(hmac_sha256(p + t, macKey)) }
    /// Keys:      encKey = SHA256("enc:" + appSecret),  macKey = SHA256("mac:" + appSecret)
    /// </summary>
    public class CryptoEnvelope
    {
        private readonly byte[] _encKey;
        private readonly byte[] _macKey;
        private const long MaxDriftSeconds = 300;   // keep in sync with the server
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

        public CryptoEnvelope(string appSecret)
        {
            if (string.IsNullOrEmpty(appSecret)) throw new ArgumentException("App secret cannot be empty.");
            using (var sha = SHA256.Create())
            {
                _encKey = sha.ComputeHash(Encoding.UTF8.GetBytes("enc:" + appSecret));
                _macKey = sha.ComputeHash(Encoding.UTF8.GetBytes("mac:" + appSecret));
            }
        }

        /// <summary>Wrap a plain JSON string in an encrypted {p,t,s} envelope.</summary>
        public string Encrypt(string innerJson)
        {
            var iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(iv);

            byte[] cipher;
            using (var aes = Aes.Create())
            {
                aes.Key = _encKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var enc = aes.CreateEncryptor())
                {
                    var plain = Encoding.UTF8.GetBytes(innerJson);
                    cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
                }
            }

            var combined = new byte[iv.Length + cipher.Length];
            Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
            Buffer.BlockCopy(cipher, 0, combined, iv.Length, cipher.Length);

            string p = Convert.ToBase64String(combined);
            long t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string s = HmacHex(p + t);

            return Json.Serialize(new Dictionary<string, object> { { "p", p }, { "t", t }, { "s", s } });
        }

        /// <summary>Verify + decrypt a {p,t,s} envelope; returns the inner JSON string.</summary>
        public string Decrypt(string envelopeJson)
        {
            var env = Json.Deserialize<Dictionary<string, object>>(envelopeJson);
            string p = Convert.ToString(env["p"]);
            long t = Convert.ToInt64(env["t"]);
            string s = Convert.ToString(env["s"]);

            string expected = HmacHex(p + t);
            if (!FixedTimeEquals(Encoding.ASCII.GetBytes(expected),
                                 Encoding.ASCII.GetBytes((s ?? "").ToLowerInvariant())))
                throw new InvalidOperationException("HMAC verification failed");

            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - t) > MaxDriftSeconds)
                throw new InvalidOperationException("Request expired (replay protection)");

            var combined = Convert.FromBase64String(p);
            if (combined.Length <= 16) throw new InvalidOperationException("Malformed ciphertext");

            var iv = new byte[16];
            Buffer.BlockCopy(combined, 0, iv, 0, 16);
            var cipher = new byte[combined.Length - 16];
            Buffer.BlockCopy(combined, 16, cipher, 0, cipher.Length);

            using (var aes = Aes.Create())
            {
                aes.Key = _encKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var dec = aes.CreateDecryptor())
                    return Encoding.UTF8.GetString(dec.TransformFinalBlock(cipher, 0, cipher.Length));
            }
        }

        /// <summary>True when the string looks like an encrypted {p,t,s} envelope
        /// (vs a plain-JSON error the server returns before crypto runs).</summary>
        public static bool IsEnvelope(string json)
        {
            try
            {
                var o = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
                return o != null && o.ContainsKey("p") && o.ContainsKey("t") && o.ContainsKey("s");
            }
            catch { return false; }
        }

        private string HmacHex(string message)
        {
            using (var h = new HMACSHA256(_macKey))
                return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(message)))
                                   .Replace("-", "").ToLowerInvariant();
        }

        // .NET Framework has no CryptographicOperations.FixedTimeEquals — implement it.
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
