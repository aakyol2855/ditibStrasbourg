using System.Text.RegularExpressions;

namespace DitibStasbourg.Core.Utilities
{
    /// <summary>
    /// Static, stateless domain utility belt.
    /// All methods are pure functions — no DI, no side effects.
    /// </summary>
    public static class FormatUtils
    {
        // ─── Phone ────────────────────────────────────────────────────────────

        /// <summary>
        /// Normalizes a raw phone string to E.164-compatible digits only.
        /// Converts leading "00" to "+". Strips spaces, dashes, and parentheses.
        /// </summary>
        public static string FormatPhoneNumber(string? rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone))
                return string.Empty;

            string cleaned = Regex.Replace(rawPhone, @"[^\d+]", "");

            if (cleaned.StartsWith("00"))
                cleaned = "+" + cleaned.Substring(2);

            return cleaned;
        }

        /// <summary>
        /// Returns true if the cleaned phone number has a plausible digit count (7–15 digits).
        /// </summary>
        public static bool IsValidPhone(string? phone)
        {
            var clean = FormatPhoneNumber(phone);
            var digits = Regex.Replace(clean, @"\D", "");
            return digits.Length is >= 7 and <= 15;
        }

        // ─── TC Kimlik ────────────────────────────────────────────────────────

        /// <summary>
        /// Strips all non-digit characters from a raw TC Kimlik string.
        /// Returns empty string if the result is not exactly 11 digits.
        /// </summary>
        public static string SanitizeTCKimlik(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            var digits = Regex.Replace(raw, @"\D", "");
            return digits.Length == 11 ? digits : string.Empty;
        }

        // ─── Audit Log ────────────────────────────────────────────────────────

        /// <summary>
        /// Sanitizes and truncates an audit log payload to prevent log-injection attacks.
        /// Strips CR/LF and HTML angle brackets; caps length at <paramref name="maxLength"/>.
        /// </summary>
        public static string SafeAuditPayload(string? rawAction, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(rawAction))
                return string.Empty;

            string clean = rawAction
                .Replace("\r", "")
                .Replace("\n", " ")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

            if (clean.Length > maxLength)
                clean = clean.Substring(0, maxLength - 3) + "...";

            return clean;
        }

        // ─── String helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Truncates a string to <paramref name="maxLength"/> characters,
        /// appending "..." if truncated. Safe for null input.
        /// </summary>
        public static string Truncate(string? input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...";
        }
    }
}
