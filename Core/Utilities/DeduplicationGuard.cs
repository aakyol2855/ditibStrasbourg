using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Core.Utilities
{
    /// <summary>
    /// Systemic temporal deduplication guard.
    /// Intercepts double-click artifacts on all data-entry ingest pipelines
    /// by enforcing a 60-second idempotency window keyed on a normalized fingerprint.
    ///
    /// Wired automatically into AddShareholder, AddGorevli, and AddDernek paths.
    /// </summary>
    public static class DeduplicationGuard
    {
        private const int ThresholdSeconds = 60;

        /// <summary>
        /// Returns true if an identical submission was detected within the last 60 seconds
        /// for the given module + fingerprint combination. Registers the attempt if not duplicate.
        /// </summary>
        /// <param name="cache">Injected IMemoryCache (singleton-safe).</param>
        /// <param name="module">Module identifier, e.g. "Hissedar", "Gorevli", "Dernek".</param>
        /// <param name="fingerprint">Normalized key composed of the distinguishing entity fields.</param>
        public static bool IsDuplicate(IMemoryCache cache, string module, string fingerprint)
        {
            var cacheKey = $"dedup:{module}:{fingerprint}";
            if (cache.TryGetValue(cacheKey, out _))
                return true;

            cache.Set(cacheKey, true, TimeSpan.FromSeconds(ThresholdSeconds));
            return false;
        }

        /// <summary>
        /// Builds a normalized fingerprint from name + phone (most common deduplication axis).
        /// Strips whitespace, lowercases, and removes non-alphanumeric chars from phone.
        /// </summary>
        public static string BuildFingerprint(string name, string? phone)
        {
            var cleanName  = (name  ?? string.Empty).Trim().ToLowerInvariant();
            var cleanPhone = System.Text.RegularExpressions.Regex.Replace(phone ?? string.Empty, @"[^\d]", "");
            return $"{cleanName}|{cleanPhone}";
        }
    }
}
