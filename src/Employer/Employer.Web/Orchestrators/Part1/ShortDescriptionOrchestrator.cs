using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
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

            var vm = new ShortDescriptionViewModel
            {
                VacancyId = vacancy.Id,
                NumberOfPositions = vacancy.NumberOfPositions.HasValue ? vacancy.NumberOfPositions : null,
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

            vacancy.NumberOfPositions = m.NumberOfPositions.Value;
            vacancy.ShortDescription = m.ShortDescription;
            
            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}
