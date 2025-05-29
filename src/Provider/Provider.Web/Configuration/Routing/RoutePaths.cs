namespace Esfa.Recruit.Provider.Web.Configuration.Routing
{
    public static class RoutePaths
    {
        public const string AccountRoutePath = "{ukprn:length(8)}";
        public const string VacanciesRoutePath = AccountRoutePath + "/vacancies";
        public const string VacancyRoutePath = "vacancies/{vacancyId:guid}";
        public const string AccountVacancyRoutePath = AccountRoutePath + "/" + VacancyRoutePath;

        public const string AccountApplicationReviewsRoutePath = AccountVacancyRoutePath + "/applications";

        public const string ApplicationReviewPath = "applications/{applicationReviewId:guid}";
        public const string AccountApplicationReviewRoutePath = AccountVacancyRoutePath + "/" + ApplicationReviewPath;
        public const string AccessDeniedPath = "/error/403";
        public const string ExceptionHandlingPath = "/error/handle";

        //Reports
        public const string ReportsRoutePath = AccountRoutePath + "/reports";
        public const string ReportsDashboardRoutePath = ReportsRoutePath + "/dashboard";
        public const string ProviderApplicationsReportRoutePath = ReportsRoutePath + "/provider-applications";
        public const string ReportRoutePath = ReportsRoutePath + "/{reportId:guid}";
        public const string ReportDownloadCsvRoutePath = ReportsRoutePath + "/{reportId:guid}/download-csv";
    }
}