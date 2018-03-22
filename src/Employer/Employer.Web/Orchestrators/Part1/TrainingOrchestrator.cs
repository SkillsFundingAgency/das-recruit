using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    using System;
    using System.Threading.Tasks;
    using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
    using Esfa.Recruit.Vacancies.Client.Domain;
    using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
    using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
    using Esfa.Recruit.Employer.Web.Extensions;
    using Esfa.Recruit.Vacancies.Client.Application.Validation;
    using Microsoft.Extensions.Logging;

    public class TrainingOrchestrator : EntityValidatingOrchestrator<Vacancy, TrainingViewModel>
    {
        private readonly IVacancyClient _client;

        public TrainingOrchestrator(IVacancyClient client, ILogger<TrainingOrchestrator> logger) : base(logger)
        {
            _client = client;
        }
        
        public async Task<TrainingViewModel> GetTrainingViewModelAsync(Guid vacancyId)
        {
            var vacancyTask = _client.GetVacancyForEditAsync(vacancyId);
            var programmesTask = _client.GetApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id,
                SelectedProgrammeId = vacancy.Programme?.Id,
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
            var vm = await GetTrainingViewModelAsync(m.VacancyId);

            vm.ClosingDay = m.ClosingDay;
            vm.ClosingMonth = m.ClosingMonth;
            vm.ClosingYear = m.ClosingYear;

            vm.StartDay = m.StartDay;
            vm.StartMonth = m.StartMonth;
            vm.StartYear = m.StartYear;

            return vm;
        }

        public async Task<OrchestratorResponse> PostTrainingEditModelAsync(TrainingEditModel m)
        {
            var vacancyTask = _client.GetVacancyForEditAsync(m.VacancyId);
            var programmesTask = _client.GetApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programme = programmesTask.Result.Programmes.Single(p => p.Id == m.SelectedProgrammeId);
            
            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.ClosingDate = m.ClosingDate.AsDateTimeUk();
            vacancy.StartDate = m.StartDate.AsDateTimeUk();
            
            vacancy.Programme = new Programme
            {
                Id = programme.Id,
                Title = programme.Title,
                TrainingType = programme.ApprenticeshipType,
                Level = programme.Level,
                LevelName = ((ProgrammeLevel)programme.Level).GetDisplayName()
            };
            
            return await BuildOrchestratorResponse(() =>_client.UpdateVacancyAsync(vacancy, VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme, false));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TrainingViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingViewModel>();

            mappings.Add(e => e.Programme, vm => vm.SelectedProgrammeId);
            mappings.Add(e => e.StartDate, vm => vm.StartDate);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);

            return mappings;
        }
    }
}
