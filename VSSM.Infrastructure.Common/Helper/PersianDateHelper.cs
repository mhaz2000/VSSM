using System;
using System.Globalization;

namespace VSSM.Infrastructure.Common.Helper
{
    public static class PersianDateHelper
    {
        private static readonly string _format = "yy/MM/dd";

        /// <summary>
        /// Converts a Persian short date string (yy/MM/dd) to Gregorian DateTime.
        /// </summary>
        public static DateTime? ToDateTime(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParseExact(
                    dateString,
                    _format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Converts DateTime? to Persian short date string (yy/MM/dd).
        /// </summary>
        public static string? ToStringDate(DateTime? date)
        {
            if (date == null)
                return null;

            return date.Value.ToString(_format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Validates if the input matches the pattern yy/MM/dd.
        /// </summary>
        public static bool IsValidFormat(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return false;

            return DateTime.TryParseExact(
                dateString,
                _format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }
    }
}
