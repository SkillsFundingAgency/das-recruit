using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class ShortDescriptionOrchestrator
    {
        private readonly IVacancyClient _client;

        public ShortDescriptionOrchestrator(IVacancyClient client)
        {
            _client = client;
        }
        
        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new ShortDescriptionViewModel
            {
                VacancyId = vacancy.Id,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                ShortDescription = vacancy.ShortDescription
            };

            return vm;
        }

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(ShortDescriptionEditModel m)
        {
            var vm = await GetShortDescriptionViewModelAsync(m.VacancyId);

            vm.NumberOfPositions = m.NumberOfPositions;
            vm.ShortDescription = m.ShortDescription;

            return vm;
        }

        public async Task PostShortDescriptionEditModelAsync(ShortDescriptionEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            if (!string.IsNullOrEmpty(m.NumberOfPositions))
            {
                int.TryParse(m.NumberOfPositions, out var numberOfPositions);
                vacancy.NumberOfPositions = numberOfPositions;
            }
            
            vacancy.ShortDescription = m.ShortDescription;
            
            await _client.UpdateVacancyAsync(vacancy, VacancyValidations.NumberOfPostions & VacancyValidations.ShortDescription, false);
        }
    }
}
