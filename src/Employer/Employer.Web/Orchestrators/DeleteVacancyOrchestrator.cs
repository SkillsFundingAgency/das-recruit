using System;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator
    {
        private readonly IVacancyClient _client;

        public DeleteVacancyOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<DeleteViewModel> GetDeleteViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new DeleteViewModel
            {
                Title = vacancy.Title,
                ConfirmDeletion = false
            };

            return vm;
        }

        public async Task<bool> TryDeleteVacancyAsync(DeleteEditModel m)
        {
            return await _client.DeleteVacancyAsync(m.VacancyId);
        }
    }
}
