using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class ConsiderationsOrchestrator : VacancyValidatingOrchestrator<ConsiderationsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.OtherRequirements;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public ConsiderationsOrchestrator(ILogger<ConsiderationsOrchestrator> logger, IRecruitVacancyClient vacancyClient, IReviewSummaryService reviewSummaryService, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Considerations_Get);
            
            var vm = new ConsiderationsViewModel
            {
                Title = vacancy.Title,
                OtherRequirements = vacancy.OtherRequirements,
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetConsiderationsFieldIndicators());
            }

            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);

            return vm;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(ConsiderationsEditModel m)
        {
            var vm = await GetConsiderationsViewModelAsync((VacancyRouteModel)m);

            vm.OtherRequirements = m.OtherRequirements;

            return vm;
        }

        public async Task<OrchestratorResponse> PostConsiderationsEditModelAsync(ConsiderationsEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Considerations_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.OtherRequirements,
                FieldIdResolver.ToFieldId(v => v.OtherRequirements),
                vacancy,
                (v) => { return v.OtherRequirements = m.OtherRequirements; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel>();

            mappings.Add(e => e.OtherRequirements, vm => vm.OtherRequirements);

            return mappings;
        }
    }
}
