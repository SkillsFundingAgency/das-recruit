using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TrainingProviderOrchestrator
    {
        private readonly IVacancyClient _client;

        public TrainingProviderOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task<ConfirmViewModel> GetConfirmViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new ConfirmViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }
    }
}
