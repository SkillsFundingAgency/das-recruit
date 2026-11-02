namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetAccountLegalEntitiesRequest : IGetApiRequest
{
    private readonly long _accountId;

    public GetAccountLegalEntitiesRequest(long accountId)
    {
        _accountId = accountId;
    }

    public string GetUrl => $"employeraccounts/{_accountId}/legalentities";
}