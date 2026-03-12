using System;
using System.Globalization;

namespace NotionConnect.Versioning
{
    public static class VersionNaming
    {
        /// <summary>
        /// Generates the next version name given a prefix, mode, and last sequence number.
        /// </summary>
        public static string Generate(string prefix, string mode, int lastSeq)
        {
            prefix = string.IsNullOrWhiteSpace(prefix) ? "v" : prefix.Trim();
            mode = string.IsNullOrWhiteSpace(mode) ? "sequential" : mode.Trim().ToLowerInvariant();

            int next = lastSeq + 1;
            string date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            switch (mode)
            {
                case "date":
                    return $"{prefix}_{date}";

                case "date+seq":
                    return $"{prefix}_{date}_{next:D3}";

                case "sequential":
                default:
                    return $"{prefix}_v{next:D3}";
            }
        }

        /// <summary>
        /// Returns a preview name without needing the last sequence number.
        /// </summary>
        public static string Preview(string prefix, string mode)
            => Generate(prefix, mode, 0);
    }
}