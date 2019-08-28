namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public static class CommunicationQueueNames
    {
        public const string CommunicationRequests = "communication-requests-queue";
        public const string AggregateCommunicationRequests = "aggregate-communication-requests-queue";
        public const string AggregateCommunicationComposeRequests = "aggregate-communication-composer-requests-queue";
        public const string CommunicationMessageDispatcher = "communication-messages-dispatcher-queue";
    }
}