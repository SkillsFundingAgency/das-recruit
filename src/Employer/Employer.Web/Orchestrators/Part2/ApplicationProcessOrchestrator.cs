using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : VacancyValidatingOrchestrator<ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ApplicationProcessOrchestrator(IEmployerVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            IOptions<ExternalLinksConfiguration> externalLinks, ILogger<ApplicationProcessOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _externalLinks = externalLinks.Value;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.ApplicationProcess_Get);

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                FindAnApprenticeshipUrl = _externalLinks.FindAnApprenticeshipUrl,
                ApplicationMethod = vacancy.ApplicationMethod,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetApplicationProcessFieldIndicators());
            }

            return vm;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(ApplicationProcessEditModel m)
        {
            var vm = await GetApplicationProcessViewModelAsync((VacancyRouteModel)m);

            vm.ApplicationMethod = m.ApplicationMethod;
            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.ApplicationProcess_Post);

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ApplicationMethod,
                FieldIdResolver.ToFieldId(v => v.ApplicationMethod),
                vacancy, 
                (v) => { return v.ApplicationMethod = m.ApplicationMethod; });

            var hasSelectedApplyThroughFaa = m.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ApplicationInstructions,
                FieldIdResolver.ToFieldId(v => v.ApplicationInstructions),
                vacancy,
                (v) => { return v.ApplicationInstructions = hasSelectedApplyThroughFaa ? null : m.ApplicationInstructions; });

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ApplicationUrl,
                FieldIdResolver.ToFieldId(v => v.ApplicationUrl),
                vacancy,
                (v) => { return v.ApplicationUrl = hasSelectedApplyThroughFaa ? null : m.ApplicationUrl; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel>
            {
                { e => e.ApplicationMethod, vm => vm.ApplicationMethod },
                { e => e.ApplicationUrl, vm => vm.ApplicationUrl },
                { e => e.ApplicationInstructions, vm => vm.ApplicationInstructions }
            };

            return mappings;
        }
    }
}
