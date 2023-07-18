namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class GetProviderRequest : IGetApiRequest
    {
        private readonly long _ukPrn;

        public GetProviderRequest(long ukPrn)
        {
            _ukPrn = ukPrn;
        }

        public string GetUrl => $"providers/{_ukPrn}";
    }
}