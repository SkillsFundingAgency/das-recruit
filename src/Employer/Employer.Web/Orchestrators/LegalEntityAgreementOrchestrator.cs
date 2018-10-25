using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class LegalEntityAgreementOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;

        public LegalEntityAgreementOrchestrator(
            IEmployerVacancyClient client, 
            ILegalEntityAgreementService legalEntityAgreementService)
        {
            _client = client;
            _legalEntityAgreementService = legalEntityAgreementService;
        }

        public async Task<LegalEntityAgreementSoftStopViewModel> GetLegalEntityAgreementSoftStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.LegalEntityAgreement_SoftStop_Get);
            
            return new LegalEntityAgreementSoftStopViewModel
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId),
                LegalEntityName = vacancy.EmployerName,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };
        }

        public async Task<LegalEntityAgreementHardStopViewModel> GetLegalEntityAgreementHardStopViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.LegalEntityAgreement_SoftStop_Get);

            return new LegalEntityAgreementHardStopViewModel
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId),
            };
        }
    }
}
