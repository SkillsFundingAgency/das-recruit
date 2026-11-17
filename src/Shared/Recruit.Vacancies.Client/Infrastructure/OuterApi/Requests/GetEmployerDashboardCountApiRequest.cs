namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetEmployerDashboardCountApiRequest(long AccountId) : IGetApiRequest
{
    public string GetUrl
    {
        get
        {
            return $"employerAccounts/{AccountId}/dashboard";
        }
    }
}