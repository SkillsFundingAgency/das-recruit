using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestrator : VacancyValidatingOrchestrator<VacancyDescriptionEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription | VacancyRuleSet.OutcomeDescription;
        private readonly IEmployerVacancyClient _client;

        public VacancyDescriptionOrchestrator(IEmployerVacancyClient client, ILogger<VacancyDescriptionOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new VacancyDescriptionViewModel
            {
                Title = vacancy.Title,
                VacancyDescription = vacancy.Description,
                TrainingDescription = vacancy.TrainingDescription,
                OutcomeDescription = vacancy.OutcomeDescription
            };

            return vm;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyDescriptionEditModel m)
        {
            var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId };
            var vm = await GetVacancyDescriptionViewModelAsync(vrm);

            vm.VacancyDescription = m.VacancyDescription;
            vm.TrainingDescription = m.TrainingDescription;
            vm.OutcomeDescription = m.OutcomeDescription;

            return vm;
        }

        public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.Description = m.VacancyDescription;
            vacancy.TrainingDescription = m.TrainingDescription;
            vacancy.OutcomeDescription = m.OutcomeDescription;
            
            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValdationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel>();

            mappings.Add(e => e.Description, vm => vm.VacancyDescription);
            mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);
            mappings.Add(e => e.OutcomeDescription, vm => vm.OutcomeDescription);

            return mappings;
        }
    }
}
