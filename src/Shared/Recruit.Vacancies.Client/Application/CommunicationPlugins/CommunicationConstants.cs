namespace Esfa.Recruit.Vacancies.Client.Application.CommunicationPlugins
{
    public static class CommunicationConstants
    {
        public const string ServiceName = "VacancyServices.Recruit";

        public static class EntityTypes
        {
            public const string Vacancy = nameof(Vacancy);
        }

        public static class RequestType
        {
            public const string VacancyReferred = "VacancyReferred";
        }
    }
}