using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ProviderAgreementOrchestrator : Controller
    {
        private readonly IPasAccountClient _pasAccountClient;

        public ProviderAgreementOrchestrator(IPasAccountClient pasAccountClient)
        {
            _pasAccountClient = pasAccountClient;
        }

        public Task<bool> HasAgreementAsync(long ukprn)
        {
            return _pasAccountClient.HasAgreementAsync(ukprn);
        }
    }
}
