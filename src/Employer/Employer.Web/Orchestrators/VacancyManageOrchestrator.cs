using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.Wage;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IEmployerVacancyClient _client;

        public VacancyManageOrchestrator(ILogger<VacancyManageOrchestrator> logger, DisplayVacancyViewModelMapper vacancyDisplayMapper, IEmployerVacancyClient client) : base(logger)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetViewModelForManageVacancy(VacancyRouteModel vrm, DateTime proposedClosingDate, DateTime proposedStartDate)
        {
            var vacancy = await GetVacancy(vrm);

            var viewModel = new ManageVacancyViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);

            if (proposedClosingDate != DateTime.MinValue)
                viewModel.ProposedClosingDate = proposedClosingDate;

            if (proposedStartDate != DateTime.MinValue)
                viewModel.ProposedStartDate = proposedStartDate;

            return viewModel;
        }

        public async Task<OrchestratorResponse> UpdatePublishedVacancyAsync(ProposedChangesEditModel m, VacancyUser user)
        {
            var vacancy = await GetVacancy(m);

            vacancy.ClosingDate = m.ProposedClosingDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.StartDate = m.ProposedStartDate.AsDateTimeUk()?.ToUniversalTime();
            
            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValdationRules),
                v => _client.UpdatePublishedVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel>
            {
                { e => e.StartDate, vm => vm.ProposedStartDate },
                { e => e.ClosingDate, vm => vm.ProposedClosingDate }
            };

            return mappings;
        }
    }
}