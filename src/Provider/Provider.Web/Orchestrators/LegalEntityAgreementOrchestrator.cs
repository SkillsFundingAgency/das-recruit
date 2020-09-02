using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class LegalEntityAgreementOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;

        public LegalEntityAgreementOrchestrator(
            IProviderVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILegalEntityAgreementService legalEntityAgreementService)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _legalEntityAgreementService = legalEntityAgreementService;
        }

        public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _client, _vacancyClient, vrm, RouteNames.LegalEntityAgreement_HardStop_Get);

            return new LegalEntityAgreementHardStopViewModel
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId),
                LegalEntityName = vacancy.LegalEntityName
            };
        }
    }
}
