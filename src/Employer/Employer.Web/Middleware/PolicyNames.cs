namespace Esfa.Recruit.Employer.Web.Middleware;

public static class PolicyNames
{
    public static string HasEmployerAccountPolicyName => nameof(HasEmployerAccountPolicyName);

    public static string HasEmployerOwnerOrTransactorAccount => nameof(HasEmployerOwnerOrTransactorAccount);
}