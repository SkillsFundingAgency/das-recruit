namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;

public static class StringExtensions
{
    public static string AsWholeMoneyAmount(this string value)
    {
        return value.Replace(".00", "");
    }
    public static string AsPerYear(this string value)
    {
        return $"{value} a year";
    }
}