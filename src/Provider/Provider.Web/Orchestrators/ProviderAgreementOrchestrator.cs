using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class ProviderAgreementOrchestrator(IGetProviderStatusClient pasAccountProvider) : Controller
{
    public async Task<bool> HasAgreementAsync(long ukprn)
    {
        var result = await pasAccountProvider.GetProviderStatus(ukprn);
        return result.CanAccessService;
    }
}