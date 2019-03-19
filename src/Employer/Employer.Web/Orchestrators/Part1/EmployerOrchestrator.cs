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

            return vm;
        }

        public Task<OrchestratorResponse> PostEmployerEditModelAsync(EmployerEditModel m, VacancyUser user)
        {
            return SaveOrganisation(m, m.SelectedOrganisationId, user);
        }

        public async Task<OrchestratorResponse> SaveOrganisation(VacancyRouteModel vacancyRouteModel, long? legalEntityId, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vacancyRouteModel, RouteNames.Employer_Post);
            var employerVacancyInfoTask = _client.GetEditVacancyInfoAsync(vacancyRouteModel.EmployerAccountId);

            await Task.WhenAll(vacancyTask, employerVacancyInfoTask);
            var vacancy = vacancyTask.Result;
            var employerVacancyInfo = employerVacancyInfoTask.Result;

            var selectedOrganisation = employerVacancyInfo.LegalEntities.SingleOrDefault(x => x.LegalEntityId == legalEntityId);

            vacancy.LegalEntityId = legalEntityId.GetValueOrDefault();
            vacancy.LegalEntityName = selectedOrganisation?.Name;
            //clear location in case the legal entity has changed
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = null;

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user));
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
