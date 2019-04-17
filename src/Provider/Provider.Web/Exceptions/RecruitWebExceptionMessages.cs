namespace Esfa.Recruit.Provider.Web.Exceptions
{
    public static class RecruitWebExceptionMessages
    {
        public const string RouteNotValidForVacancy = "Invalid route for vacancy. Requested route:{0} Redirecting to route:{1}";
        public const string ProviderMissingPermission = "Provider {0} has not been assigned the Recruitment permission by any employers.";
    }
}