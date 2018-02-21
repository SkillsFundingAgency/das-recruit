using Esfa.Recruit.Employer.Web.ViewModels.WageAndHours;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class WageAndHoursOrchestrator
    {
        private readonly IVacancyClient _client;

        public WageAndHoursOrchestrator(IVacancyClient client)
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
