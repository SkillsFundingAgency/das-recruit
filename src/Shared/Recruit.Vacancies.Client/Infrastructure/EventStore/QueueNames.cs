namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore
{
    public static class QueueNames
    {
        public const string DomainEventsQueueName = "domain-events-queue";
        public const string GenerateSingleEmployerDashboardQueueName = "generate-employer-dashboard-queue";
        public const string GenerateAllEmployerDashboardQueueName = "generate-all-employer-dashboards-queue";
        public const string GenerateLiveVacanciesQueueName = "generate-live-vacancies-queue";
        public const string ApplicationSubmittedQueueName = "application-submitted-queue";
        public const string ApplicationWithdrawnQueueName = "application-withdrawn-queue";
    }
}