using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : EntityValidatingOrchestrator<Vacancy, ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationUrl | VacancyRuleSet.ApplicationInstructions;
        private readonly IEmployerVacancyClient _client;

        public ApplicationProcessOrchestrator(IEmployerVacancyClient client, ILogger<ApplicationProcessOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm.VacancyId, vrm.EmployerAccountId, RouteNames.ApplicationProcess_Get);

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl
            };

            return vm;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(ApplicationProcessEditModel m)
        {
            var vm = await GetApplicationProcessViewModelAsync((VacancyRouteModel)m);

            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m.VacancyId, m.EmployerAccountId, RouteNames.ApplicationProcess_Post);
            
            vacancy.ApplicationInstructions = m.ApplicationInstructions;
            vacancy.ApplicationUrl = m.ApplicationUrl;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel>();

            mappings.Add(e => e.ApplicationUrl, vm => vm.ApplicationUrl);
            mappings.Add(e => e.ApplicationInstructions, vm => vm.ApplicationInstructions);

            return mappings;
        }
    }
}
