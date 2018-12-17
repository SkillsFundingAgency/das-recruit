namespace Esfa.Recruit.Provider.Web.Configuration.Routing
{
    public static class RoutePaths
    {
        public const string AccountRoutePath = "/{ukprn:length(8)}/recruit";
        public const string Services = "services";
        public const string AccessDeniedPath = "/error/403";
        public const string ExceptionHandlingPath = "/error/handle";
    }
}
