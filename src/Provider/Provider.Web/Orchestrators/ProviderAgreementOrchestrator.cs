using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ProviderAgreementOrchestrator : Controller
    {
        private readonly IPasAccountProvider _pasAccountProvider;

        public ProviderAgreementOrchestrator(IPasAccountProvider pasAccountProvider)
        {
            _pasAccountProvider = pasAccountProvider;
        }

        public Task<bool> HasAgreementAsync(long ukprn)
        {
            return _pasAccountProvider.HasAgreementAsync(ukprn);
        }
    }
}
