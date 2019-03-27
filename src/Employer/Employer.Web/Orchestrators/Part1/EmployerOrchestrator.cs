using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.LegalEntityName;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILogger<EmployerOrchestrator> _logger;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;

        public EmployerOrchestrator(
            IEmployerVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILogger<EmployerOrchestrator> logger,
            ILegalEntityAgreementService legalEntityAgreementService)
            : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _logger = logger;
            _legalEntityAgreementService = legalEntityAgreementService;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(VacancyRouteModel vrm)
        {
            var getEmployerDataTask = _client.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getVacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Employer_Get);
            await Task.WhenAll(getEmployerDataTask, getVacancyTask);
            var employerData = getEmployerDataTask.Result;
            var vacancy = getVacancyTask.Result;

            var vm = new EmployerViewModel
            {                
                Organisations = BuildLegalEntityViewModels(employerData, vrm.EmployerAccountId),
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

            if(vacancy.EmployerNameOption.HasValue)
                vm.VacancyEmployerInfoModel.EmployerNameOption = vacancy.EmployerNameOption.Value.GetModelOption();

            return vm;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel>();

            mappings.Add(e => e.LegalEntityName, vm => vm.SelectedOrganisationId);

            return mappings;
        }

        private IEnumerable<OrganisationViewModel> BuildLegalEntityViewModels(EditVacancyInfo info, string employerAccountId)
        {
            if (info == null || !info.LegalEntities.Any())
            {
                _logger.LogError("No legal entities found for {employerAccountId}", employerAccountId);
                return new List<OrganisationViewModel>(); // TODO: Can we carry on without a list of legal entities.
            }

            return info.LegalEntities.Select(MapLegalEntitiesToOrgs).ToList();
        }
        
        private OrganisationViewModel MapLegalEntitiesToOrgs(LegalEntity data)
        {
            return new OrganisationViewModel { Id = data.LegalEntityId, Name = data.Name};
        }
        
    }
}
