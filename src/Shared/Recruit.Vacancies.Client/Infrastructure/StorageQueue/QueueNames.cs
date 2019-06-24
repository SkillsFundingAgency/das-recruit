namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public static class QueueNames
    {
        public const string DomainEventsQueueName = "domain-events-queue";
        public const string GenerateSingleEmployerDashboardQueueName = "generate-employer-dashboard-queue";
        public const string GenerateSingleProviderDashboardQueueName = "generate-provider-dashboard-queue";
        public const string GenerateBlockedEmployersQueueName = "generate-blocked-employers-data-queue";
        public const string GenerateAllEmployerDashboardQueueName = "generate-all-employer-dashboards-queue";
        public const string GenerateAllProviderDashboardQueueName = "generate-all-provider-dashboards-queue";
        public const string GenerateAllVacancyAnalyticsSummariesQueueName = "generate-all-vacancy-analytics-summaries-queue";
        public const string GenerateAllVacancyApplicationsQueueName = "generate-all-vacancy-applications-queue";
        public const string GeneratePublishedVacanciesQueueName = "generate-published-vacancies-queue";
        public const string GenerateVacancyAnalyticsQueueName = "generate-vacancy-analytics-summary";
        public const string ApplicationSubmittedQueueName = "application-submitted-queue";
        public const string ApplicationWithdrawnQueueName = "application-withdrawn-queue";
        public const string CandidateDeleteQueueName = "candidate-delete-queue";
        public const string ReportQueueName = "report-queue";
        public const string UpdateApprenticeProgrammesQueueName = "update-apprenticeship-programmes-queue";
        public const string UpdateQaDashboardQueueName = "update-qa-dashboard-queue";
        public const string DeleteReportsQueueName = "delete-reports-queue";
        public const string VacancyStatusQueueName = "vacancy-status-queue";
        public const string UpdateBankHolidaysQueueName = "update-bank-holidays-queue";
        public const string UpdateUserAccountQueueName = "update-user-account-queue";
    }
}