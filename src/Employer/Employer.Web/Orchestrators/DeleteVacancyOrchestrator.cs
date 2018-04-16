using System;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;

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
            var vacancy = await _client.GetVacancyAsync(vacancyId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new DeleteViewModel
            {
                Title = vacancy.Title,
            };

            return vm;
        }

        public async Task<bool> TryDeleteVacancyAsync(DeleteEditModel m)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            return await _client.DeleteVacancyAsync(m.VacancyId);
        }
    }
}
