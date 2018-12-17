namespace Esfa.Recruit.Qa.Web.Configuration.Routing
{
    public static class RoutePaths
    {
        public const string VacancyReviewsRoutePath = "reviews/{reviewId:guid}";
        public const string AccessDeniedPath = "/error/403";
        public const string ExceptionHandlingPath = "/error/handle";
    }
}