namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public static class QueueNames
    {
        public const string DomainEventsQueueName = "domain-events-queue";
        public const string GenerateSingleEmployerDashboardQueueName = "generate-employer-dashboard-queue";
        public const string GenerateSingleProviderDashboardQueueName = "generate-provider-dashboard-queue";
        public const string GenerateAllBlockedOrganisationsQueueName = "generate-all-blocked-organisations-queue";
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
        public const string TransferVacanciesFromProviderQueueName = "transfer-vacancies-from-provider-queue";
        public const string TransferVacanciesToLegalEntityQueueName = "transfer-vacancy-to-legalentity-queue";
        public const string VacancyStatusQueueName = "vacancy-status-queue";
        public const string UpdateBankHolidaysQueueName = "update-bank-holidays-queue";
        public const string UpdateEmployerUserAccountQueueName = "update-employer-user-account-queue";
        public const string DeleteStaleQueryStoreDocumentsQueueName = "delete-stale-query-store-documents-queue";
        public const string DeleteStaleVacanciesQueueName = "delete-stale-vacancies-queue";
        public const string BeginMigrationQueueName = "begin-migration";
        public const string DataMigrationQueueName = "data-migration";
        public const string CommunicationsHouseKeepingQueueName = "communications-house-keeping-queue";
        public const string UpdateProvidersQueueName = "update-providers-queue";
    }
}