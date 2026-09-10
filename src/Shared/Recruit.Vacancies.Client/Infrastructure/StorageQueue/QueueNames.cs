namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public static class QueueNames
    {
        public const string DomainEventsQueueName = "domain-events-queue";
        public const string ApplicationSubmittedQueueName = "application-submitted-queue";
        public const string ApplicationWithdrawnQueueName = "application-withdrawn-queue";
        public const string CandidateDeleteQueueName = "candidate-delete-queue";
        public const string ReportQueueName = "report-queue";
        public const string DeleteReportsQueueName = "delete-reports-queue";
        public const string VacancyStatusQueueName = "vacancy-status-queue";
        public const string UpdateEmployerUserAccountQueueName = "update-employer-user-account-queue";
        public const string UpdateProvidersQueueName = "update-providers-queue";
        public const string UpdateProviderInfoQueueName = "update-provider-info-queue";
    }
}