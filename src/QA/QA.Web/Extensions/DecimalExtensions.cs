namespace Esfa.Recruit.Qa.Web.Extensions
{
    //NOTE: Duplicated from Employer solution
    public static class DecimalExtensions
    {
        public static string AsMoney(this decimal dec)
        {
            return $"{dec:N2}".Replace(".00", "");
        }
    }
}