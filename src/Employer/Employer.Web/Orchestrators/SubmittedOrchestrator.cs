using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class SubmittedOrchestrator
    {
        private readonly IVacancyClient _client;

        public SubmittedOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                Title = vacancy.Title,
                VacancyReference = "12345678"
            };

            return vm;
        }
    }
}
