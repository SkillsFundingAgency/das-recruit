using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : VacancyValidatingOrchestrator<ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public ApplicationProcessOrchestrator(IRecruitVacancyClient vacancyClient,
            IOptions<ExternalLinksConfiguration> externalLinks, ILogger<ApplicationProcessOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService,
            IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _externalLinks = externalLinks.Value;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(TaskListViewModel model)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.ApplicationProcess_Get);

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                FindAnApprenticeshipUrl = _externalLinks.FindAnApprenticeshipUrl,
                ApplicationMethod = vacancy.ApplicationMethod,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                Ukprn = model.Ukprn,
                VacancyId = model.VacancyId
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetApplicationProcessFieldIndicators());
            }

            vm.IsTaskList = model.IsTaskList;
            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy) && !model.IsTaskList;
            return vm;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(ApplicationProcessEditModel m)
        {
            var vm = await GetApplicationProcessViewModelAsync((TaskListViewModel)m);
            vm.ApplicationMethod = m.ApplicationMethod;
            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;
            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.ApplicationProcess_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ApplicationMethod,
                FieldIdResolver.ToFieldId(v => v.ApplicationMethod),
                vacancy,
                (v) => { return v.ApplicationMethod = m.ApplicationMethod; });
            
            var hasSelectedApplyThroughFaa = m.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            
            if (hasSelectedApplyThroughFaa && vacancy.HasSubmittedAdditionalQuestions)
            {
                // Make sure these are cleared if already set as they are not required for external application methods.
                vacancy.HasSubmittedAdditionalQuestions = false;
                if (vacancy.AdditionalQuestion1 is not null)
                {
                    SetVacancyWithProviderReviewFieldIndicators(
                        vacancy.AdditionalQuestion1,
                        FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1),
                        vacancy, 
                        v => v.AdditionalQuestion1 = null);
                }

                if (vacancy.AdditionalQuestion2 is not null)
                {
                    SetVacancyWithProviderReviewFieldIndicators(
                        vacancy.AdditionalQuestion2,
                        FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2),
                        vacancy,
                        v => v.AdditionalQuestion2 = null);
                }
            }

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ApplicationInstructions,
                FieldIdResolver.ToFieldId(v => v.ApplicationInstructions),
                vacancy,
                (v) => { return v.ApplicationInstructions = hasSelectedApplyThroughFaa ? null : m.ApplicationInstructions; });

            SetVacancyWithProviderReviewFieldIndicators(
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
