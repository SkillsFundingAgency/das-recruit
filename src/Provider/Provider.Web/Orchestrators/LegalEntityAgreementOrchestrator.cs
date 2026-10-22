using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Shared.Web.Services;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class LegalEntityAgreementOrchestrator(
        ILegalEntityAgreementService legalEntityAgreementService,
        IUtility utility)
    {
        public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.LegalEntityAgreement_HardStop_Get);

            return new LegalEntityAgreementHardStopViewModel
            {
                HasLegalEntityAgreement = await legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId),
                LegalEntityName = vacancy.LegalEntityName,
                Ukprn = vrm.Ukprn
            };
        }
    }
}