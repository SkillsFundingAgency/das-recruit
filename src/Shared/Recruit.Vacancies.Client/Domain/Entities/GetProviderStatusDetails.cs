using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class GetProviderStatusDetails : IGetApiRequest
    {
        private readonly long _ukprn;

        public GetProviderStatusDetails(long ukprn)
        {
            _ukprn = ukprn;
        }

        public string GetUrl => $"provideraccounts/{_ukprn}";
    }
}
