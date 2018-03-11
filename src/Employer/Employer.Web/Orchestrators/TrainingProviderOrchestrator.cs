using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
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

        public async Task<IndexViewModel> GetIndexViewModel(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException($"The vacancy '{vacancy.Title}' is not available for editing.");

            var vm = new IndexViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.Ukprn,
                ProviderName = vacancy.ProviderName,
                ProviderAddress = vacancy.ProviderAddress
            };

            return vm;
        }

        public async Task<ConfirmViewModel> GetConfirmViewModel(IndexEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException($"The vacancy '{vacancy.Title}' is not available for editing.");

            var confirmViewModel = new ConfirmViewModel
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

        public async Task PostConfirmEditModelAsync(ConfirmEditModel m)
        {
            var vacancyTask = _client.GetVacancyForEditAsync(m.VacancyId);
            var providerDetailTask = _providerService.GetProviderDetailAsync(long.Parse(m.Ukprn));

            Task.WaitAll(new Task[] { vacancyTask, providerDetailTask });

            var vacancy = vacancyTask.Result;
            var providerDetail = providerDetailTask.Result;

            vacancy.Ukprn = providerDetail.Ukprn;
            vacancy.ProviderName = providerDetail.ProviderName;
            vacancy.ProviderAddress = providerDetail.ProviderAddress;

            await _client.UpdateVacancyAsync(vacancy, canUpdateQueryStore: false);
        }

        public async Task<bool> ConfirmProviderExists(long ukprn)
        {
            return await _providerService.ExistsAsync(ukprn);
        }
    }
}