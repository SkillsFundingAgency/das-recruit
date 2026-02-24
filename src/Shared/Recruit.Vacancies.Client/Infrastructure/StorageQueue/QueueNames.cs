namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public static class QueueNames
    {
        public const string DomainEventsQueueName = "domain-events-queue";
        public const string GenerateAllBlockedOrganisationsQueueName = "generate-all-blocked-organisations-queue";
        public const string GenerateAllVacancyApplicationsQueueName = "generate-all-vacancy-applications-queue";
        public const string ApplicationSubmittedQueueName = "application-submitted-queue";
        public const string ApplicationWithdrawnQueueName = "application-withdrawn-queue";
        public const string CandidateDeleteQueueName = "candidate-delete-queue";
        public const string ReportQueueName = "report-queue";
        public const string DeleteReportsQueueName = "delete-reports-queue";
        public const string TransferVacanciesFromProviderQueueName = "transfer-vacancies-from-provider-queue";
        public const string TransferVacanciesFromEmployerReviewToQAReviewQueueName = "transfer-vacancies-from-employer-review-qa-review-queue";
        public const string TransferVacanciesToLegalEntityQueueName = "transfer-vacancy-to-legalentity-queue";
        public const string VacancyStatusQueueName = "vacancy-status-queue";
        public const string UpdateEmployerUserAccountQueueName = "update-employer-user-account-queue";
        public const string DeleteStaleQueryStoreDocumentsQueueName = "delete-stale-query-store-documents-queue";
        public const string DeleteStaleVacanciesQueueName = "delete-stale-vacancies-queue";
        public const string CommunicationsHouseKeepingQueueName = "communications-house-keeping-queue";
        public const string UpdateProvidersQueueName = "update-providers-queue";
        public const string UpdateProviderInfoQueueName = "update-provider-info-queue";
    }
}