namespace Esfa.Recruit.Employer.Web.Middleware;

public static class PolicyNames
{
    public static string HasEmployerAccountPolicyName
    {
        get
        {
            return nameof(HasEmployerAccountPolicyName);
        }
    }
    public static string HasEmployerOwnerOrTransactorAccount
    {
        get
        {
            return nameof(HasEmployerOwnerOrTransactorAccount);
        }
    }
}