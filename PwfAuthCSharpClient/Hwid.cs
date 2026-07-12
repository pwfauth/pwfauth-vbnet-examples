using System;
using System.Security.Cryptography;
using System.Text;

namespace PwfAuthCSharpClient
{
    /// <summary>
    /// A stable per-machine hardware id sent at login (the server binds the license
    /// to it). Derived from machine + user + OS — good enough to show the flow; a
    /// production app would combine more hardware signals (disk / CPU / MAC).
    /// Matches the VB example's Hwid.GetId(), so the same box yields the same id.
    /// </summary>
    public static class Hwid
    {
        public static string GetId()
        {
            string seed = string.Join("|",
                Environment.MachineName,
                Environment.UserName,
                Environment.OSVersion.Platform.ToString(),
                Environment.ProcessorCount.ToString());

            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)))
                                   .Replace("-", "").Substring(0, 32).ToLowerInvariant();
        }
    }
}
