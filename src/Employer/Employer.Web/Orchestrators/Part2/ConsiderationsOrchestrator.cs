using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ConsiderationsOrchestrator : EntityValidatingOrchestrator<Vacancy, ConsiderationsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;
        private readonly IEmployerVacancyClient _client;

        public ConsiderationsOrchestrator(ILogger<ConsiderationsOrchestrator> logger, IEmployerVacancyClient client) : base(logger)
        {
            _client = client;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Considerations_Get);
            
            var vm = new ConsiderationsViewModel
            {
                Title = vacancy.Title,
                ThingsToConsider = vacancy.ThingsToConsider,
            };

            return vm;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(ConsiderationsEditModel m)
        {
            var vm = await GetConsiderationsViewModelAsync((VacancyRouteModel)m);

            vm.ThingsToConsider = m.ThingsToConsider;

            return vm;
        }

        public async Task<OrchestratorResponse> PostConsiderationsEditModelAsync(ConsiderationsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Considerations_Post);
            
            vacancy.ThingsToConsider = m.ThingsToConsider;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
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
