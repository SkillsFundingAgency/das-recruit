using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

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

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new TitleViewModel
            {
                VacancyId = vacancy.Id,
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task<TitleViewModel> GetTitleViewModelAsync(TitleEditModel m)
        {
            TitleViewModel vm;

            if (m.VacancyId.HasValue)
            {
                vm = await GetTitleViewModelAsync(m.VacancyId.Value);
            }
            else
            {
                vm = GetTitleViewModel();
            }

            vm.Title = m.Title;

            return vm;
        }

        public async Task<Guid> PostTitleEditModelAsync(TitleEditModel vm, string user)
        {
            if (!vm.VacancyId.HasValue)
            {
                var id = await _client.CreateVacancyAsync(vm.Title, vm.EmployerAccountId, user);

                return id;
            }

            var vacancy = await _client.GetVacancyForEditAsync(vm.VacancyId.Value);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.Title = vm.Title;

            await _client.UpdateVacancyAsync(vacancy);

            return vacancy.Id;
        }
    }
}
