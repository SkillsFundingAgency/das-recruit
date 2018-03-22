using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TrainingProviderOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly ITrainingProviderService _providerService;

        public TrainingProviderOrchestrator(IVacancyClient client, ITrainingProviderService providerService)
        {
            _client = client;
            _providerService = providerService;
        }

        public async Task<SelectTrainingProviderViewModel> GetIndexViewModel(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.Ukprn
            };

            return vm;
        }

        public async Task<ConfirmTrainingProviderViewModel> GetConfirmViewModel(SelectTrainingProviderEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var confirmViewModel = new ConfirmTrainingProviderViewModel
            {
                Title = vacancy.Title
            };

            if (long.TryParse(m.Ukprn, out var ukprn) && ukprn != vacancy.Ukprn)
            {
                var providerDetail = await _providerService.GetProviderDetailAsync(ukprn);
                confirmViewModel.Ukprn = providerDetail.Ukprn;
                confirmViewModel.ProviderName = providerDetail.ProviderName;
                confirmViewModel.ProviderAddress = providerDetail.ProviderAddress;
            }
            else
            {
                confirmViewModel.Ukprn = vacancy.Ukprn.Value;
                confirmViewModel.ProviderName = vacancy.ProviderName;
                confirmViewModel.ProviderAddress = vacancy.ProviderAddress;
            }

            return confirmViewModel;
        }

        public async Task PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel m)
        {
            var vacancyTask = _client.GetVacancyForEditAsync(m.VacancyId);
            var providerDetailTask = _providerService.GetProviderDetailAsync(long.Parse(m.Ukprn));

            Task.WaitAll(new Task[] { vacancyTask, providerDetailTask });

            var vacancy = vacancyTask.Result;
            var providerDetail = providerDetailTask.Result;

            vacancy.Ukprn = providerDetail.Ukprn;
            vacancy.ProviderName = providerDetail.ProviderName;
            vacancy.ProviderAddress = providerDetail.ProviderAddress;

            await _client.UpdateVacancyAsync(vacancy, VacancyRuleSet.None, canUpdateQueryStore: false);
        }

        public Task<bool> ConfirmProviderExists(long ukprn)
        {
            return _providerService.ExistsAsync(ukprn);
        }
    }
}