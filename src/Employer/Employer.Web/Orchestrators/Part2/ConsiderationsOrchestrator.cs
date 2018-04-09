using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ConsiderationsOrchestrator : EntityValidatingOrchestrator<Vacancy, ConsiderationsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;
        private readonly IVacancyClient _client;
        private readonly ILogger<ConsiderationsOrchestrator> _logger;

        public ConsiderationsOrchestrator(ILogger<ConsiderationsOrchestrator> logger, IVacancyClient client) : base(logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new ConsiderationsViewModel
            {
                Title = vacancy.Title,
                ThingsToConsider = vacancy.ThingsToConsider,
            };

            return vm;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(ConsiderationsEditModel m)
        {
            var vm = await GetConsiderationsViewModelAsync(m.VacancyId);

            vm.ThingsToConsider = m.ThingsToConsider;            

            return vm;
        }

        public async Task<OrchestratorResponse> PostConsiderationsEditModelAsync(ConsiderationsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            vacancy.ThingsToConsider = m.ThingsToConsider;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel>();

            mappings.Add(e => e.ThingsToConsider, vm => vm.ThingsToConsider);

            return mappings;
        }
    }
}
