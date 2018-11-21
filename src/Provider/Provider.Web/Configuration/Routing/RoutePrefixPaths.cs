namespace Esfa.Recruit.Provider.Web.Configuration.Routing
{
    public static class RoutePrefixPaths
    {
        public const string AccountRoutePath = "/{ukprn:minlength(8)}/recruit";
        public const string Services = "services";
        public const string AccessDeniedPath = "/error/403";
    }
}
