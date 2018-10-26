namespace Esfa.Recruit.Shared.Web.Configuration
{
    public static class EnvironmentNames
    {
        public const string Development = "DEVELOPMENT";
        public const string AT = "AT";
        public const string TEST = "TEST";
        public const string DEMO = "DEMO";
        public const string PREPROD = "PREPROD";
        public const string PROD = "PROD";

        public static string[] GetTestEnvironmentNames() => new []{ Development, AT, TEST, DEMO };
        
        public static string GetTestEnvironmentNamesCommaDelimited() => string.Join(",", Development, AT, TEST, DEMO);

        public static string GetNonProdEnvironmentNamesCommaDelimited() => string.Join(",", Development, AT, TEST, DEMO, PREPROD);
    }
}