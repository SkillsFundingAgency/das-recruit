namespace Esfa.Recruit.Employer.Web.Configuration.Routing
{
    public static class RoutePrefixPaths
    {        
        public const string VacancyRoutePath = "vacancies/{vacancyId:guid}";
        public const string AccountRoutePath = "accounts/{employerAccountId:minlength(6)}";
        public const string AccountVacancyRoutePath = AccountRoutePath + "/" + VacancyRoutePath;
    }
}
