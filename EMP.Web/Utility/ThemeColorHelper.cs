using System.Globalization;
using System.Text.RegularExpressions;

namespace EMP.Web.Utility
{
    /// <summary>Builds a CSS override that re-skins the app's primary color from a hex value.</summary>
    public static class ThemeColorHelper
    {
        /// <summary>App default (Vuexy purple).</summary>
        public const string Default = "#696CFF";

        private static readonly Regex HexPattern = new("^#?[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        /// <summary>
        /// Returns CSS overriding the primary color (and its button/label variants), or an
        /// empty string when the value is missing/invalid/the default.
        /// </summary>
        public static string BuildPrimaryOverride(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || !HexPattern.IsMatch(hex.Trim()))
            {
                return string.Empty;
            }

            hex = hex.Trim();
            if (!hex.StartsWith("#")) hex = "#" + hex;

            if (string.Equals(hex, Default, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);

            string Darken(double factor)
            {
                int C(int c) => (int)Math.Max(0, Math.Round(c * (1 - factor)));
                return $"#{C(r):x2}{C(g):x2}{C(b):x2}";
            }

            var hover = Darken(0.12);
            var active = Darken(0.20);

            return $@"
:root, [data-bs-theme=""light""], [data-bs-theme=""dark""] {{
    --bs-primary: {hex};
    --bs-primary-rgb: {r},{g},{b};
}}
.btn-primary {{
    --bs-btn-bg: {hex}; --bs-btn-border-color: {hex};
    --bs-btn-hover-bg: {hover}; --bs-btn-hover-border-color: {hover};
    --bs-btn-active-bg: {active}; --bs-btn-active-border-color: {active};
    --bs-btn-disabled-bg: {hex}; --bs-btn-disabled-border-color: {hex};
}}
.bg-primary {{ background-color: {hex} !important; }}
.text-primary {{ color: {hex} !important; }}
.btn-label-primary {{ color: {hex}; background-color: rgba({r},{g},{b},0.16); }}
.bg-label-primary {{ color: {hex} !important; background-color: rgba({r},{g},{b},0.16) !important; }}
.avatar-initial.bg-label-primary {{ color: {hex} !important; }}
.menu-vertical .menu-item.active > .menu-link {{ color: {hex}; }}
.form-check-input:checked {{ background-color: {hex}; border-color: {hex}; }}
.page-item.active .page-link {{ background-color: {hex}; border-color: {hex}; }}
";
        }
    }
}
