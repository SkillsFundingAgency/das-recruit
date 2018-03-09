using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TitleVacancy;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TitleOrchestrator
    {
        private readonly IVacancyClient _client;

        public TitleOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public TitleViewModel GetTitleViewModel()
        {
            var vm = new TitleViewModel();
            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task<Guid> PostTitleViewModelAsync(TitleViewModel vm)
        {
            if (!vm.VacancyId.HasValue)
            {
                var id = await _client.CreateVacancyAsync(vm.Title, vm.EmployerAccountId);

                return id;
            }

            var vacancy = await _client.GetVacancyForEditAsync(vm.VacancyId.Value);

            vacancy.Title = vm.Title?.Trim();

            await _client.UpdateVacancyAsync(vacancy);

            return vacancy.Id;
        }
    }
}
