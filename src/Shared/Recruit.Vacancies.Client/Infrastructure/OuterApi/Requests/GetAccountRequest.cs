namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetAccountRequest(long accountId) : IGetApiRequest
{
    public string GetUrl => $"employeraccounts/{accountId}";
}