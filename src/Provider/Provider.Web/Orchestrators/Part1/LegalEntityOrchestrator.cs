using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityOrchestrator
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly ILogger<EmployerOrchestrator> _logger;

        public LegalEntityOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            ILogger<EmployerOrchestrator> logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _logger = logger;
        }

        public async Task<LegalEntityViewModel> GetLegalEntityViewModelAsync(VacancyRouteModel vrm, long ukprn)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.LegalEntity_Get);

            var vm = new LegalEntityViewModel
            {                
                Organisations = await GetLegalEntityViewModelsAsync(ukprn, vacancy.EmployerAccountId),
                SelectedOrganisationId = vacancy.LegalEntityId,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            vm.VacancyEmployerInfoModel = new VacancyEmployerInfoModel()
            {
                VacancyId = vacancy.Id,
                LegalEntityId = vacancy.LegalEntityId == 0 ? (long?)null : vacancy.LegalEntityId
            };

            if (vm.VacancyEmployerInfoModel.LegalEntityId == null && vm.HasOnlyOneOrganisation)
            {
                vm.VacancyEmployerInfoModel.LegalEntityId = vm.Organisations.First().Id;
            }

            if (vacancy.EmployerNameOption.HasValue)
            {
                vm.VacancyEmployerInfoModel.EmployerIdentityOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
                vm.VacancyEmployerInfoModel.AnonymousName = vacancy.IsAnonymous ? vacancy.EmployerName : null;
                vm.VacancyEmployerInfoModel.AnonymousReason = vacancy.IsAnonymous ? vacancy.AnonymousReason : null;
            }

            return vm;
        }
       
        private OrganisationViewModel ConvertToOrganisationViewModel(LegalEntity data)
        {
            return new OrganisationViewModel { Id = data.LegalEntityId, Name = data.Name};
        }

        private async Task<List<OrganisationViewModel>> GetLegalEntityViewModelsAsync(long ukprn, string employerAccountId)
        {
            var result = new List<OrganisationViewModel>();

            var info = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);  

            if (info == null || !info.LegalEntities.Any())
            {
                _logger.LogWarning("No legal entities found for {employerAccountId}", employerAccountId);
                return result; 
            }

            return info.LegalEntities.Select(ConvertToOrganisationViewModel).ToList();
        }
    }
}