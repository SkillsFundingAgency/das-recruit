using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class ShortDescriptionOrchestrator : VacancyValidatingOrchestrator<ShortDescriptionEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions | VacancyRuleSet.ShortDescription;
        private readonly IEmployerVacancyClient _client;

        public ShortDescriptionOrchestrator(IEmployerVacancyClient client, ILogger<ShortDescriptionOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

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
            var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId };
            var vm = await GetShortDescriptionViewModelAsync(vrm);

            vm.NumberOfPositions = m.NumberOfPositions;
            vm.ShortDescription = m.ShortDescription;

            return vm;
        }

        public async Task<OrchestratorResponse> PostShortDescriptionEditModelAsync(ShortDescriptionEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.NumberOfPositions = int.TryParse(m.NumberOfPositions, out var numberOfPositions) ? numberOfPositions : default(int?);
            vacancy.ShortDescription = m.ShortDescription;

            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
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
