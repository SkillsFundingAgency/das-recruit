namespace Esfa.Recruit.Vacancies.Client.Application.Exceptions
{
    public static class ExceptionMessages
    {        
        public const string VacancyUnauthorisedAccess = "The employer account {0} cannot access employer account {1} vacancy '{2} ({3})'.";
        public const string VacancyWithReferenceNotFound = "Unable to find vacancy with reference: {0}.";
        public const string VacancyWithIdNotFound = "Unable to find vacancy with id: {0}.";
    }
}
