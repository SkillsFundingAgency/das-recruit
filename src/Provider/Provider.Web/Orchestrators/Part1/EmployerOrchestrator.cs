using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployersEditViewModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAccountId | VacancyRuleSet.TrainingProvider;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public EmployerOrchestrator(ILogger<EmployerOrchestrator> logger, 
            IProviderVacancyClient providerVacancyClient, IRecruitVacancyClient recruitVacancyClient)
            : base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployersEditViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployersEditViewModel>();

            mappings.Add(e => e.EmployerAccountId, vm => vm.SelectedEmployerId);

            return mappings;
        }
        public async Task<EmployersViewModel> GetEmployersViewModelAsync(VacancyRouteModel vacancyRouteModel)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn);

            var vm = new EmployersViewModel
            {
                PageInfo = new PartOnePageInfoViewModel(),
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel {Id = e.Id, Name = e.Name})
            };

            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostEmployerEditViewModelAsync(
            VacancyRouteModel vacancyRouteModel, EmployersEditViewModel viewModel, VacancyUser user)
        {
            if (!viewModel.VacancyId.HasValue) // Create if it's a new vacancy
            {
                 var newVacancy = new Vacancy
                {
                    TrainingProvider = new TrainingProvider{Ukprn = viewModel.Ukprn},
                    EmployerAccountId = viewModel.SelectedEmployerId,
                    EmployerName = await GetEmployerNameAsync(viewModel.Ukprn, viewModel.SelectedEmployerId)
                };

                return await ValidateAndExecute(
                    newVacancy, 
                    v => _recruitVacancyClient.Validate(v, ValidationRules),
                    async v => await _providerVacancyClient.CreateVacancyAsync(SourceOrigin.ProviderWeb, 
                        viewModel.SelectedEmployerId, viewModel.Ukprn, user, UserType.Provider));
            }

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vacancyRouteModel, RouteNames.Employer_Post);

            vacancy.EmployerAccountId = viewModel.SelectedEmployerId;
            vacancy.EmployerName = await GetEmployerNameAsync(viewModel.Ukprn, viewModel.SelectedEmployerId);

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _providerVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    return v.Id;
                }
            );
        }

        private async Task<string> GetEmployerNameAsync(long ukprn, string employerId)
        {
            var providerInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);

            return providerInfo.Employers.FirstOrDefault(e => e.Id == employerId)?.Name;
        }
    }
}