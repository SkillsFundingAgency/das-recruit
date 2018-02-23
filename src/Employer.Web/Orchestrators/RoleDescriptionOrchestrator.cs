using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class RoleDescriptionOrchestrator
    {
        private readonly IVacancyClient _client;

        public RoleDescriptionOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                CurrentVacancyTitle = vacancy.Title,
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task PostIndexEditModelAsync(IndexEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);
            
            vacancy.Title = m.Title;

            await _client.UpdateVacancyAsync(vacancy);
        }
    }
}
