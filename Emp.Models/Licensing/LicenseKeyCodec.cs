using System.Security.Cryptography;
using System.Text;

namespace Emp.Models.Licensing
{
    /// <summary>
    /// Pure, dependency-free encode/decode for license keys, shared by the API (verify),
    /// the dev generate endpoint, and the offline vendor generator console. A key is
    /// "base64url(payload).base64url(HMACSHA256(payload, secret))" where payload is
    /// "Type|yyyy-MM-dd".
    /// </summary>
    public static class LicenseKeyCodec
    {
        public static string Generate(string type, DateTime expiresAt, byte[] secret)
        {
            var payloadBytes = Encoding.UTF8.GetBytes($"{type}|{expiresAt:yyyy-MM-dd}");
            return Base64Url(payloadBytes) + "." + Sign(payloadBytes, secret);
        }

        public static bool TryVerify(string key, byte[] secret, out string type, out DateTime expiresAt)
        {
            type = string.Empty;
            expiresAt = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(key)) return false;
            var parts = key.Split('.');
            if (parts.Length != 2) return false;

            byte[] payloadBytes;
            try { payloadBytes = FromBase64Url(parts[0]); }
            catch { return false; }

            var expectedSig = Sign(payloadBytes, secret);
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSig), Encoding.UTF8.GetBytes(parts[1])))
            {
                return false;
            }

            var fields = Encoding.UTF8.GetString(payloadBytes).Split('|');
            if (fields.Length != 2) return false;
            if (!DateTime.TryParse(fields[1], out var parsed)) return false;

            type = fields[0];
            expiresAt = parsed;
            return true;
        }

        private static string Sign(byte[] payloadBytes, byte[] secret)
        {
            using var hmac = new HMACSHA256(secret);
            return Base64Url(hmac.ComputeHash(payloadBytes));
        }

        private static string Base64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromBase64Url(string s)
        {
            var padded = s.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
