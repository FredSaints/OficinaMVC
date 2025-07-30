namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides helper methods for formatting and displaying views.
    /// </summary>
    public static class ViewHelper
    {
        private static readonly TimeZoneInfo PortugalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        /// <summary>
        /// Converts a nullable UTC <see cref="DateTime"/> to a formatted string in the local Portuguese time zone.
        /// </summary>
        /// <param name="utcDate">The UTC <see cref="DateTime"/> to format.</param>
        /// <param name="format">The format string (e.g., "g" for short date/time, "D" for long date).</param>
        /// <returns>A formatted string, or an empty string if the date is null.</returns>
        public static string FormatUtcDate(DateTime? utcDate, string format = "g")
        {
            if (!utcDate.HasValue)
            {
                return string.Empty;
            }

            var localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate.Value, PortugalTimeZone);
            return localDate.ToString(format);
        }
    }
}