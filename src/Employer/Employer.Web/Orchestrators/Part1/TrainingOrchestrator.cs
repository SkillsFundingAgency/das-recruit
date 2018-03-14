namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    using System;
    using System.Threading.Tasks;
    using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
    using Esfa.Recruit.Vacancies.Client.Domain;
    using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
    using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
    using System.Globalization;
    using Esfa.Recruit.Employer.Web.Extensions;

    public class TrainingOrchestrator
    {
        private readonly IVacancyClient _client;

        public TrainingOrchestrator(IVacancyClient client)
        {
            _client = client;
        }
        
        public async Task<TrainingViewModel> GetTrainingViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id
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

        public async Task PostTrainingEditModelAsync(TrainingEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.ClosingDate = m.ClosingDate.AsDateTimeUk();
            vacancy.StartDate = m.StartDate.AsDateTimeUk();

            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}
