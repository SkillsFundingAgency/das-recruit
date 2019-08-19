namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public static class CommunicationConstants
    {
        public const string ServiceName = "VacancyServices.Recruit";

        public const string UserType = "VacancyServices.Recruit.User";

        public static class EntityTypes
        {
            public const string Vacancy = nameof(Vacancy);
            public const string ApprenticeshipServiceUrl = nameof(ApprenticeshipServiceUrl);
        }

        public static class RequestType
        {
            public const string VacancyRejected = nameof(VacancyRejected);
            public const string ApplicationSubmitted = nameof(ApplicationSubmitted);
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
            }
        }

        public static class TemplateIds
        {
            public const string VacancyRejected = "RecruitV2_VacancyRejected";
            public const string ApplicationSubmittedImmediate = "RecruitV2_NewApplicationImmediate";
            public const string VacancyWithdrawnByQa = "RecruitV2_VacancyWithdrawnByQa";
        }
    }
}