namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetAlertsByAccountIdApiRequest(long AccountId, string UserId) : IGetApiRequest
{
    public string GetUrl => $"employerAccounts/{AccountId}/alerts?userId={UserId}";
}