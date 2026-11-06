using System;
using TimeZoneConverter;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;

public static class DateTimeExtensions
{
    private const string DisplayDateFormat = "d MMM yyyy";
    private const string TimeDisplayFormat = "h.mm";
    private const string DateTimeDisplayFormat = DisplayDateFormat + " " + TimeDisplayFormat;

    public static string AsGdsDate(this DateTime date)
    {
        return date.ToString(DisplayDateFormat);
    }

    public static string AsGdsDate(this DateTime? date)
    {
        return date?.ToString(DisplayDateFormat);
    }

    public static string AsGdsDateTime(this DateTime date)
    {
        return $"{date.ToString(DateTimeDisplayFormat)}{date.ToString("tt").ToLower()}";
    }

    public static string AsGdsTime(this DateTime date)
    {
        return $"{date.ToString(TimeDisplayFormat)}{date.ToString("tt").ToLower()}";
    }

    public static string AsInputHintDisplayDate(this DateTime date)
    {
        return date.ToString("dd MM yyyy");
    }

    public static string ToMonthYearString(this DateTime date)
    {
        return date.Year == DateTime.MinValue.Year ? "Current" : date.ToString("MMM yyyy");
    }

    public static string ToMonthNameYearString(this DateTime date, bool isToDate)
    {
        var separator = isToDate ? "to " : "";
        return date.Year == DateTime.MinValue.Year ? "Onwards" : $"{separator}{date:MMMM yyyy}";
    }

    public static string ToDayMonthYearString(this DateTime date)
    {
        return date.Year == DateTime.MinValue.Year ? "Current" : date.ToString("d MMMM yyyy");
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

    public static DateTime ToUkTime(this DateTime datetime)
    {
        var ukTimezone = TZConvert.GetTimeZoneInfo("GMT Standard Time");
        return TimeZoneInfo.ConvertTime(datetime, ukTimezone);
    }

    public static string ToFullDateTimeString(this DateTime datetime)
    {
        return datetime.ToString("dddd d MMMM yyy");
    }
}