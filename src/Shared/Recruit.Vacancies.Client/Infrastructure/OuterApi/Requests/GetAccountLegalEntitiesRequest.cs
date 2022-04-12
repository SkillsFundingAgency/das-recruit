namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetAccountLegalEntitiesRequest : IGetApiRequest
    {
        private readonly string _accountId;

        public GetAccountLegalEntitiesRequest(string accountId)
        {
            _accountId = accountId;
        }

        public string GetUrl => $"employeraccounts/{_accountId}/legalentities";
    }
}