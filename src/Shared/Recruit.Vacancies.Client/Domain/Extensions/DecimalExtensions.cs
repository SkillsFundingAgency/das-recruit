namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;

public static class DecimalExtensions
{
    public static string AsMoney(this decimal dec)
    {
        return $"{dec:N2}".Replace(".00", "");
    }
}