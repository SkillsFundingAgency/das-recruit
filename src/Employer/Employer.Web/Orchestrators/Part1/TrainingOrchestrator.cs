using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TrainingOrchestrator : EntityValidatingOrchestrator<Vacancy, TrainingEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate;
        private readonly IEmployerVacancyClient _client;
        private async Task<Vacancy> GetVacancy(Guid id) => await _client.GetVacancyAsync(id);

        public TrainingOrchestrator(IEmployerVacancyClient client, ILogger<TrainingOrchestrator> logger) : base(logger)
        {
            _client = client;
        }
        
        public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(GetVacancy, vrm, RouteNames.Training_Get);
            var programmesTask = _client.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;
            
            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id,
                SelectedProgrammeId = vacancy.ProgrammeId,
                Programmes = programmes.ToViewModel()
            };

            if (vacancy.ClosingDate.HasValue)
            {
                vm.ClosingDay = $"{vacancy.ClosingDate.Value.Day:00}";
                vm.ClosingMonth = $"{vacancy.ClosingDate.Value.Month:00}";
                vm.ClosingYear = $"{vacancy.ClosingDate.Value.Year}";
            }

            if (vacancy.StartDate.HasValue)
            {
                vm.StartDay = $"{vacancy.StartDate.Value.Day:00}";
                vm.StartMonth = $"{vacancy.StartDate.Value.Month:00}";
                vm.StartYear = $"{vacancy.StartDate.Value.Year}";
            }

            return vm;
        }

        public async Task<TrainingViewModel> GetTrainingViewModelAsync(TrainingEditModel m)
        {
            var vm = await GetTrainingViewModelAsync((VacancyRouteModel)m);

            vm.ClosingDay = m.ClosingDay;
            vm.ClosingMonth = m.ClosingMonth;
            vm.ClosingYear = m.ClosingYear;

            vm.StartDay = m.StartDay;
            vm.StartMonth = m.StartMonth;
            vm.StartYear = m.StartYear;

            vm.SelectedProgrammeId = m.SelectedProgrammeId;

            return vm;
        }

        public async Task<OrchestratorResponse> PostTrainingEditModelAsync(TrainingEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(GetVacancy, m, RouteNames.Training_Post);
            
            vacancy.ClosingDate = m.ClosingDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.StartDate = m.StartDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.ProgrammeId = m.SelectedProgrammeId;
            
            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValdationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel>();

            mappings.Add(e => e.ProgrammeId, vm => vm.SelectedProgrammeId);
            mappings.Add(e => e.StartDate, vm => vm.StartDate);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);

            return mappings;
        }
    }
}
