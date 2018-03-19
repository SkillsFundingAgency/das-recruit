using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleViewModel>
    {
        private readonly IVacancyClient _client;
        private readonly ILogger<TitleOrchestrator> _logger;

        public TitleOrchestrator(IVacancyClient client, ILogger<TitleOrchestrator> logger)
        {
            _logger = logger;
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

        public async Task<OrchestratorResponse<Guid>> PostTitleEditModelAsync(TitleEditModel vm, string user)
        {
            if (!vm.VacancyId.HasValue)
            {
                var id = await _client.CreateVacancyAsync(vm.Title, vm.EmployerAccountId, user);

                return new OrchestratorResponse<Guid>(id);;
            }

            var vacancy = await _client.GetVacancyForEditAsync(vm.VacancyId.Value);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.Title = vm.Title;

            try
            {
                await _client.UpdateVacancyAsync(vacancy, VacancyValidations.Title);
            }
            catch (EntityValidationException ex)
            {
                _logger.LogDebug("Vacancy update failed validation: {ValidationErrors}", ex.ValidationResult);

                return new OrchestratorResponse<Guid>(ex.ValidationResult);
            }

            return new OrchestratorResponse<Guid>(vacancy.Id);
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleViewModel> DefineMappings()
        {
            return null;
        }
    }
}
