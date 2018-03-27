using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyDescriptionOrchestrator
    {
        private readonly IVacancyClient _client;

        public VacancyDescriptionOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new VacancyDescriptionViewModel
            {
                Title = vacancy.Title,
                VacancyDescription = vacancy.Description,
                TrainingDescription = vacancy.TrainingDescription,
                OutcomeDescription = vacancy.OutcomeDescription
            };

            return vm;
        }

        public async Task PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            vacancy.Description = m.VacancyDescription;
            vacancy.TrainingDescription = m.TrainingDescription;
            vacancy.OutcomeDescription = m.OutcomeDescription;

            await _client.UpdateVacancyAsync(vacancy);
        }
    }
}
