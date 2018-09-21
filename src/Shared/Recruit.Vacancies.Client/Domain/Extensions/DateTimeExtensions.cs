using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        private const string DisplayDateFormat = "dd MMM yyyy";
        private const string TimeDisplayFormat = "h.mm";
        private const string DateTimeDisplayFormat = DisplayDateFormat + " " + TimeDisplayFormat;

        public static string AsGdsDate(this DateTime date)
        {
            return date.ToString(DisplayDateFormat);
        }

        public static string AsGdsDateTime(this DateTime date)
        {
            return $"{date.ToString(DateTimeDisplayFormat)}{date.ToString("tt").ToLower()}";
        }

        public static string AsGdsTime(this DateTime date)
        {
            return $"{date.ToString(TimeDisplayFormat)}{date.ToString("tt").ToLower()}";
        }

        public static string ToMonthYearString(this DateTime date)
        {
            return date.Year == DateTime.MinValue.Year ? "Current" : date.ToString("MMM yyyy");
        }

        public static string GetShortTimeElapsed(this DateTime? value, DateTime currentTime)
        {
            if (value == null) return string.Empty;
            var diff = currentTime - value.Value;

            if (diff < TimeSpan.FromMinutes(1))
                return null;

            var hours = diff.Hours > 0 ? $"{diff.Hours}h" : string.Empty;
            var minutes = diff.Minutes > 0 ? $"{diff.Minutes}m" : string.Empty;

            return $"{hours} {minutes}";
        }
    }
}