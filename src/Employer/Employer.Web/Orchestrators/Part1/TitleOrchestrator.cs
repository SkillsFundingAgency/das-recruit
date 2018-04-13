using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class TitleOrchestrator : EntityValidatingOrchestrator<Vacancy, TitleEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
        private readonly IEmployerVacancyClient _client;
        private readonly ILogger<TitleOrchestrator> _logger;

        public TitleOrchestrator(IEmployerVacancyClient client, ILogger<TitleOrchestrator> logger) : base(logger)
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
            var vacancy = await _client.GetVacancyAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
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
            if (!vm.VacancyId.HasValue) // Create if it's a new vacancy
            {
                var newVacancy = new Vacancy { Title = vm.Title };

                return await ValidateAndExecute<Guid>(
                    newVacancy, 
                    v => _client.Validate(v, ValidationRules),
                    async v =>
                    {
                        return await _client.CreateVacancyAsync(vm.Title, vm.EmployerAccountId, user);
                    }
                );
            }

            var vacancy = await _client.GetVacancyAsync(vm.VacancyId.Value);

            if (!vacancy.CanEdit)
            {
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.Title = vm.Title;

            return await ValidateAndExecute<Guid>(
                vacancy, 
                v => _client.Validate(v, ValidationRules),
                async v =>
                {
                    await _client.UpdateVacancyAsync(vacancy);
                    return v.Id;
                }
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TitleEditModel> DefineMappings()
        {
            return null;
        }
    }
}
