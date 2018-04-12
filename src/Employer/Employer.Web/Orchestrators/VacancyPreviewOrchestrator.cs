using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;

        public VacancyPreviewOrchestrator(IVacancyClient client, DisplayVacancyViewModelMapper vacancyDisplayMapper)
        {
            _client = client;
            _vacancyDisplayMapper = vacancyDisplayMapper;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new VacancyPreviewViewModel();

            _vacancyDisplayMapper.MapFromVacancy(vm, vacancy);

            return vm;
        }

        public Task<bool> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            return _client.SubmitVacancyAsync(m.VacancyId);
        }
    }
}
