namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetAccountRequest : IGetApiRequest
{
    private readonly long _accountId;

    public GetAccountRequest(long accountId)
    {
        _accountId = accountId;
    }

    public string GetUrl => $"employeraccounts/{_accountId}";
}