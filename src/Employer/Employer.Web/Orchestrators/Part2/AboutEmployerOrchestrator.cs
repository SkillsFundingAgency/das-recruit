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
    public class AboutEmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, AboutEmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;
        private readonly IEmployerVacancyClient _client;

        public AboutEmployerOrchestrator(IEmployerVacancyClient client, ILogger<AboutEmployerOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.AboutEmployer_Get);

            var vm = new AboutEmployerViewModel
            {
                Title = vacancy.Title,
                EmployerDescription = vacancy.EmployerDescription,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl
            };

            return vm;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(AboutEmployerEditModel m)
        {
            var vm = await GetAboutEmployerViewModelAsync((VacancyRouteModel)m);

            vm.EmployerDescription = m.EmployerDescription;
            vm.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostAboutEmployerEditModelAsync(AboutEmployerEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.AboutEmployer_Post);

            vacancy.EmployerDescription = m.EmployerDescription;
            vacancy.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel>();

            mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
            mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);

            return mappings;
        }
    }
}
