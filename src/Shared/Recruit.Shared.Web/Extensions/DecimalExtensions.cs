namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class DecimalExtensions
    {
        public static string AsMoney(this decimal dec)
        {
            return $"{dec:N2}".Replace(".00", "");
        }
    }
}