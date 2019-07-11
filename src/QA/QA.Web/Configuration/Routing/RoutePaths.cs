namespace Esfa.Recruit.Qa.Web.Configuration.Routing
{
    public static class RoutePaths
    {
        public const string VacancyReviewsRoutePath = "reviews/{reviewId:guid}";
        public const string AccessDeniedPath = "/error/403";
        public const string ExceptionHandlingPath = "/error/handle";

        //Reports 
        public const string ReportsRoutePath = "/reports";
        public const string ApplicationsReportRoutePath = ReportsRoutePath + "/applications";
        public const string ReportsDashboardRoutePath = ReportsRoutePath + "/dashboard";
        public const string ReportRoutePath = ReportsRoutePath + "/{reportId:guid}";
        public const string ReportDownloadCsvRoutePath = ReportsRoutePath + "/{reportId:guid}/download-csv";
    }
}