using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator
    {
        private readonly IVacancyClient _client;

        public ApplicationProcessOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                EmployerContactName = vacancy.EmployerContactName,
                EmployerContactEmail = vacancy.EmployerContactEmail,
                EmployerContactPhone = vacancy.EmployerContactPhone
            };

            return vm;
        }

        public async Task PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.ApplicationInstructions = m.ApplicationInstructions;
            vacancy.ApplicationUrl = m.ApplicationUrl;
            vacancy.EmployerContactName = m.EmployerContactName;
            vacancy.EmployerContactEmail = m.EmployerContactEmail;
            vacancy.EmployerContactPhone = m.EmployerContactPhone;

            await _client.UpdateVacancyAsync(vacancy);
        }
    }
}
