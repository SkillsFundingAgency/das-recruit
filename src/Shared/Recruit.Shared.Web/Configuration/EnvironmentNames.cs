namespace Esfa.Recruit.Shared.Web.Configuration;

public static class EnvironmentNames
{
    private const string Development = "DEVELOPMENT";
    private const string AT = "AT";
    private const string TEST = "TEST";
    private const string TEST2 = "TEST2";
    private const string DEMO = "DEMO";
    private const string PREPROD = "PREPROD";
    private const string PROD = "PROD";

    private static string[] GetTestEnvironmentNames() => [Development, AT, TEST, TEST2];
    public static string GetTestEnvironmentNamesCommaDelimited() => string.Join(",", GetTestEnvironmentNames());
    public static string GetNonProdEnvironmentNamesCommaDelimited() => string.Join(",", Development, AT, TEST, TEST2, DEMO, PREPROD);
}