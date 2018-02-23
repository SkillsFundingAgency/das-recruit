﻿using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Storage.Client.Domain.Entities;
using Recruit.Vacancies.Client.Infrastructure.Client;
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

            var vm = new IndexViewModel
            {
                Title = vacancy.Title,
                IsSubmittable = vacancy.IsSubmittable()
            };

            return vm;
        }

        public async Task<bool> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            return await _client.SubmitVacancyAsync(m.VacancyId);
        }
    }
}
