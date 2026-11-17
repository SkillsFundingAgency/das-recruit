namespace Esfa.Recruit.Employer.Web.Configuration.Routing;

public static class RoutePaths
{
    public const string Services = "services";
    public const string AccountLevelServices = "services/{employerAccountId:minlength(6)}";
    public const string AccountRoutePath = "accounts/{employerAccountId:minlength(6)}";
    public const string VacanciesRoutePath = AccountRoutePath + "/vacancies";

    private const string VacancyRoutePath = "vacancies/{vacancyId:guid}";
    public const string AccountVacancyRoutePath = AccountRoutePath + "/" + VacancyRoutePath;

    private const string ApplicationReviewPath = "applications/{applicationReviewId:guid}";
    public const string AccountApplicationReviewRoutePath = AccountVacancyRoutePath + "/" + ApplicationReviewPath;

    private const string ApplicationReviewsPath = "applications";
    public const string AccountApplicationReviewsRoutePath = AccountVacancyRoutePath + "/" + ApplicationReviewsPath;

    public const string ExceptionHandlingPath = "/error/handle";
}