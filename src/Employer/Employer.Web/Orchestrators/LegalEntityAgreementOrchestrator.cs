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
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;

        public LegalEntityAgreementOrchestrator(
            IEmployerVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILegalEntityAgreementService legalEntityAgreementService)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _legalEntityAgreementService = legalEntityAgreementService;
        }

        public async Task<LegalEntityAgreementSoftStopViewModel> GetLegalEntityAgreementSoftStopViewModelAsync(
            VacancyRouteModel vrm, string selectedAccountLegalEntityPublicHashedId)
        {
            var vacancy = await
                Utility.GetAuthorisedVacancyForEditAsync(
                    _client, _vacancyClient, vrm, RouteNames.LegalEntityAgreement_SoftStop_Get);

            var accountLegalEntityPublicHashedId = string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId) ? selectedAccountLegalEntityPublicHashedId : vacancy.AccountLegalEntityPublicHashedId;

            LegalEntity legalEntity = await
                _legalEntityAgreementService.GetLegalEntityAsync(vrm.EmployerAccountId, accountLegalEntityPublicHashedId);

            var hasLegalEntityAgreement = await
                _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, legalEntity);
            
            return new LegalEntityAgreementSoftStopViewModel
            {                
                HasLegalEntityAgreement = hasLegalEntityAgreement,
                LegalEntityName = legalEntity.Name,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };
        }

        public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _client, _vacancyClient, vrm, RouteNames.LegalEntityAgreement_HardStop_Get);

            return new LegalEntityAgreementHardStopViewModel
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                    vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId)
            };
        }
    }
}
