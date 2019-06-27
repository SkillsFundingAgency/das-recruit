using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ProviderAgreementOrchestrator : Controller
    {
        private readonly ITrainingProviderAgreementProvider _trainingProviderAgreementProvider;

        public ProviderAgreementOrchestrator(ITrainingProviderAgreementProvider trainingProviderAgreementProvider)
        {
            _trainingProviderAgreementProvider = trainingProviderAgreementProvider;
        }

        public Task<bool> HasAgreementAsync(long ukprn)
        {
            return _trainingProviderAgreementProvider.HasAgreementAsync(ukprn);
        }
    }
}
