using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class ShortDescriptionOrchestrator : EntityValidatingOrchestrator<Vacancy, ShortDescriptionEditModel>
    {
        private readonly IVacancyClient _client;
        private readonly ILogger<ShortDescriptionOrchestrator> _logger;

        public ShortDescriptionOrchestrator(IVacancyClient client, ILogger<ShortDescriptionOrchestrator> logger) : base(logger)
        {
            _logger = logger;
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

        public async Task<OrchestratorResponse> PostShortDescriptionEditModelAsync(ShortDescriptionEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.NumberOfPositions = int.TryParse(m.NumberOfPositions, out var numberOfPositions) ? numberOfPositions : default(int?);
            vacancy.ShortDescription = m.ShortDescription;

            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, VacancyRuleSet.NumberOfPostions | VacancyRuleSet.ShortDescription),
                v => _client.UpdateVacancyAsync(vacancy, false)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel>();

            mappings.Add(e => e.NumberOfPositions, vm => vm.NumberOfPositions);
            mappings.Add(e => e.ShortDescription, vm => vm.ShortDescription);

            return mappings;
        }
    }
}
