namespace Esfa.Recruit.Vacancies.Client.Application.Communications;

public static class CommunicationConstants
{
    public const string ServiceName = "VacancyServices.Recruit";
    public const string UserType = "VacancyServices.Recruit.User";
    public const string HelpdeskPhoneNumber = Constants.HelpdeskPhoneNumber;

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
        public const string ApprenticeshipServiceUnsubscribeUrl = nameof(ApprenticeshipServiceUnsubscribeUrl);
        public const string Provider = nameof(Provider);
        public const string Employer = nameof(Employer);
        public const string ApprenticeshipServiceConfig = nameof(ApprenticeshipServiceConfig);
    }

    public static class RequestType
    {
        public const string VacancyWithdrawnByQa = nameof(VacancyWithdrawnByQa);
    }

    public static class DataItemKeys
    {
        public static class Vacancy
        {
            public const string VacancyReference = "vacancy-reference";
            public const string VacancyTitle = "vacancy-title";
            public const string EmployerName = "employer-name";
        }

        public static class ApprenticeshipService
        {
            public const string ApprenticeshipServiceUrl = "apprenticeship-service-url";
            public const string ApprenticeshipServiceUnsubscribeUrl = "apprenticeship-service-unsubscribe-url";
            public const string HelpdeskPhoneNumber = "helpdesk-number";
        }

        public static class Provider
        {
            public const string ProviderName = "provider-name";
        }
    }

    public static class TemplateIds
    {
        public const string VacancyWithdrawnByQa = "RecruitV2_VacancyWithdrawnByQa";
    }
}