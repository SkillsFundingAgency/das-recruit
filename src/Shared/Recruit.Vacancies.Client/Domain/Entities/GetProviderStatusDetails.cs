using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class GetProviderStatusDetails(long ukprn) : IGetApiRequest
    {
        public string GetUrl => $"provideraccounts/{ukprn}";
    }
}