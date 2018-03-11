﻿using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class PreviewOrchestrator
    {
        private readonly IVacancyClient _client;

        public PreviewOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException($"The vacancy '{vacancy.Title}' is not available for editing.");

            var vm = new IndexViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.Ukprn,
                ProviderName = vacancy.ProviderName,
                ProviderAddress = vacancy.ProviderAddress,
                CanSubmit = vacancy.CanSubmit
            };

            return vm;
        }

        public async Task<bool> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            return await _client.SubmitVacancyAsync(m.VacancyId);
        }
    }
}
