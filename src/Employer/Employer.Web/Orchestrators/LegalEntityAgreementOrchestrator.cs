using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class LegalEntityAgreementOrchestrator
    {
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;
        private readonly IUtility _utility;

        public LegalEntityAgreementOrchestrator(
            ILegalEntityAgreementService legalEntityAgreementService, IUtility utility)
        {
            _legalEntityAgreementService = legalEntityAgreementService;
            _utility = utility;
        }

        public async Task<LegalEntityAgreementSoftStopViewModel> GetLegalEntityAgreementSoftStopViewModelAsync(
            VacancyRouteModel vrm, string selectedAccountLegalEntityPublicHashedId)
        {
            var vacancy = await
                _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.LegalEntityAgreement_SoftStop_Get);

            var accountLegalEntityPublicHashedId = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId) ? selectedAccountLegalEntityPublicHashedId : vacancy.AccountLegalEntityPublicHashedId;

            var legalEntity = await
                _legalEntityAgreementService.GetLegalEntityAsync(vrm.EmployerAccountId, accountLegalEntityPublicHashedId);

            var hasLegalEntityAgreement = await
                _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, legalEntity);
            
            return new LegalEntityAgreementSoftStopViewModel
            {                
                HasLegalEntityAgreement = hasLegalEntityAgreement,
                LegalEntityName = legalEntity.Name,
                PageInfo = _utility.GetPartOnePageInfo(vacancy)
            };
        }

        public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.LegalEntityAgreement_HardStop_Get);

            return new LegalEntityAgreementHardStopViewModel
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId)
            };
        }
    }
}
