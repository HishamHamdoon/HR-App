using System.Text.Json;

namespace EMP.Web
{
    public class JwtHelper
    {
        public static string GetUsernameFromJwt(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            var jsonBytes = Convert.FromBase64String(PadBase64(payload));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.TryGetValue("unique_name", out var username) ? username.ToString() : null;
        }

        private static string PadBase64(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        }
    }
}
