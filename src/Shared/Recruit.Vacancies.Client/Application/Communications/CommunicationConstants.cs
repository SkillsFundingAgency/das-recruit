namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public static class CommunicationConstants
    {
        public const string ServiceName = "VacancyServices.Recruit";

        public const string UserType = "VacancyServices.Recruit.User";

        public const string HelpdeskPhoneNumber = Esfa.Recruit.Vacancies.Client.Application.Constants.HelpdeskPhoneNumber;

        public static class ParticipantResolverNames
        {
            public const string ProviderParticipantsResolverName = nameof(ProviderParticipantsResolverName);
            public const string VacancyParticipantsResolverName = nameof(VacancyParticipantsResolverName);
            public const string EmployerParticipantsResolverName = nameof(EmployerParticipantsResolverName);
        }

        public static class EntityTypes
        {
            public const string Vacancy = nameof(Vacancy);
            public const string ApprenticeshipServiceUrl = nameof(ApprenticeshipServiceUrl);
            public const string Provider = nameof(Provider);
            public const string Employer = nameof(Employer);
            public const string ApprenticeshipServiceConfig = nameof(ApprenticeshipServiceConfig);
        }

        public static class RequestType
        {
            public const string VacancyRejected = nameof(VacancyRejected);
            public const string ApplicationSubmitted = nameof(ApplicationSubmitted);
            public const string VacancyWithdrawnByQa = nameof(VacancyWithdrawnByQa);
            public const string ProviderBlockedProviderNotification = nameof(ProviderBlockedProviderNotification);
            public const string ProviderBlockedEmployerNotificationForTransferredVacancies = nameof(ProviderBlockedEmployerNotificationForTransferredVacancies);
            public const string ProviderBlockedEmployerNotificationForLiveVacancies = nameof(ProviderBlockedEmployerNotificationForLiveVacancies);
            public const string ProviderBlockedEmployerNotificationForPermissionOnly = nameof(ProviderBlockedEmployerNotificationForPermissionOnly);
        }

        public static class DataItemKeys
        {
            public static class Vacancy
            {
                public const string VacancyReference = "vacancy-reference";
                public const string VacancyTitle = "vacancy-title";
                public const string EmployerName = "employer-name";
            }

            public static class Application
            {
                public const string ApplicationsSubmittedAggregateHeader = "applications-submitted-aggregate-header";
                public const string ApplicationsSubmittedAggregateBodySnippets = "applications-submitted-aggregate-body-snippets";
            }

            public static class ApprenticeshipService
            {
                public const string ApprenticeshipServiceUrl = "apprenticeship-service-url";
                public const string HelpdeskPhoneNumber = "helpdesk-number";
            }

            public static class Provider
            {
                public const string ProviderName = "provider-name";
            }

            public static class Employer
            {
                public const string EmployerAccountId = nameof(EmployerAccountId);
                public const string VacanciesTransferredCountText = "vacancies-transferred-count-text";
            }
        }

        public static class TemplateIds
        {
            public const string VacancyRejected = "RecruitV2_VacancyRejected";
            public const string ApplicationSubmittedImmediate = "RecruitV2_NewApplicationImmediate";
            public const string ApplicationsSubmittedDigest = "RecruitV2_NewApplicationsDigest";
            public const string VacancyWithdrawnByQa = "RecruitV2_VacancyWithdrawnByQa";
            public const string ProviderBlockedProviderNotification = "RecruitV2_ProviderBlocked_ProviderNotification";
            public const string ProviderBlockedEmployerNotificationForTransferredVacancies = "RecruitV2_ProviderBlocked_EmployerNotification_ForTransferredVacancies";
            public const string ProviderBlockedEmployerNotificationForLiveVacancies = "RecruitV2_ProviderBlocked_EmployerNotification_ForLiveVacancies";
            public const string ProviderBlockedEmployerNotificationForPermissionsOnly = "RecruitV2_ProviderBlocked_EmployerNotification_ForPermissionsOnly";
        }
    }
}