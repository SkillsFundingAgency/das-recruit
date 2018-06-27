namespace Esfa.Recruit.Employer.Web.Configuration.Routing
{
    public static class RoutePrefixPaths
    {
        public const string AccountRoutePath = "accounts/{employerAccountId:minlength(6)}";

        public const string VacancyRoutePath = "vacancies/{vacancyId:guid}";
        public const string AccountVacancyRoutePath = AccountRoutePath + "/" + VacancyRoutePath;

        public const string ApplicationReviewPath = "applications/{applicationReviewId:guid}";
        public const string AccountApplicationReviewRoutePath = AccountVacancyRoutePath + "/" + ApplicationReviewPath;
    }
}
