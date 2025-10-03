using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ProviderAgreementOrchestrator : Controller
    {
        private readonly IGetProviderStatusClient _pasAccountProvider;

        public ProviderAgreementOrchestrator(IGetProviderStatusClient pasAccountProvider)
        {
            _pasAccountProvider = pasAccountProvider;
        }

        public async Task<bool> HasAgreementAsync(long ukprn)
        {
            var result = await _pasAccountProvider.GetProviderStatus(ukprn);
            return result.CanAccessService;
        }
    }
}
