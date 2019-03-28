﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

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
            VacancyRouteModel vrm, long? selectedLegalEntityId)
        {
            var vacancy = 
                await Utility.GetAuthorisedVacancyForEditAsync(
                    _client, _vacancyClient, vrm, RouteNames.LegalEntityAgreement_SoftStop_Get);

            var legalEntityId = selectedLegalEntityId.HasValue ? selectedLegalEntityId.Value : vacancy.LegalEntityId;
            
            return new LegalEntityAgreementSoftStopViewModel
            {                
                HasLegalEntityAgreement = 
                    await _legalEntityAgreementService.HasLegalEntityAgreementAsync(
                        vacancy.EmployerAccountId, legalEntityId),
                LegalEntityName = vacancy.EmployerName,
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
                    vacancy.EmployerAccountId, vacancy.LegalEntityId),
            };
        }
    }
}
