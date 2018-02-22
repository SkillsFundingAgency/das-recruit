using Esfa.Recruit.Employer.Web.ViewModels.Preview;
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
                Title = vacancy.Title
            };

            return vm;
        }
    }
}
