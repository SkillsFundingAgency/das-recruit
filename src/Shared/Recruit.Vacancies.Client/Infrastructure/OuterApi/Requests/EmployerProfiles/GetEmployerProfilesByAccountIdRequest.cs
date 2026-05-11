namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;

public record GetEmployerProfilesByAccountIdRequest(long EmployerAccountId) : IGetApiRequest
{
    public string GetUrl => $"employer/{EmployerAccountId}/profiles";
}