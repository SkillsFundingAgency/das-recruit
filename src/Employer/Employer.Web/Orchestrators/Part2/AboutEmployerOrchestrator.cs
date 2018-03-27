using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class AboutEmployerOrchestrator
    {
        private readonly IVacancyClient _client;

        public AboutEmployerOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new AboutEmployerViewModel
            {
                Title = vacancy.Title,
                EmployerDescription = vacancy.EmployerDescription,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl
            };

            return vm;
        }

        public async Task PostAboutEmployerEditModelAsync(AboutEmployerEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            vacancy.EmployerDescription = m.EmployerDescription;
            vacancy.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            await _client.UpdateVacancyAsync(vacancy);
        }
    }
}
