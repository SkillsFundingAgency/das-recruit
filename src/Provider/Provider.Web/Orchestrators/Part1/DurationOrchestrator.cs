using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class DurationOrchestrator : VacancyValidatingOrchestrator<DurationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Duration | VacancyRuleSet.WorkingWeekDescription | VacancyRuleSet.WeeklyHours;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;
        private readonly IFeature _feature;

        public DurationOrchestrator(IRecruitVacancyClient vacancyClient, 
            ILogger<DurationOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility, IFeature feature) 
            : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
            _feature = feature;
        }

        public async Task<DurationViewModel> GetDurationViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Duration_Get);

            var training = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var vm = new DurationViewModel
            {
                Title = vacancy.Title,
                Duration = vacancy.Wage?.Duration?.ToString(),
                DurationUnit = vacancy.Wage?.DurationUnit ?? DurationUnit.Month,
                WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
                WeeklyHours = $"{vacancy.Wage?.WeeklyHours:0.##}",
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                TrainingTitle = training?.Title,
                TrainingDurationMonths = training?.Duration ?? 0,
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId,
                IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy),
                MinimumApprenticeshipLength = _feature.IsFeatureEnabled("FoundationApprenticeships") ? 8 : 12 
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetDurationReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<DurationViewModel> GetDurationViewModelAsync(DurationEditModel m)
        {
            var vm = await GetDurationViewModelAsync((VacancyRouteModel)m);

            vm.Duration = m.Duration;
            vm.DurationUnit = m.DurationUnit;
            vm.WorkingWeekDescription = m.WorkingWeekDescription;
            vm.WeeklyHours = m.WeeklyHours;
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostDurationEditModelAsync(DurationEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Duration_Post);

            if(vacancy.Wage == null)
                vacancy.Wage = new Wage();

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.Duration,
                FieldIdResolver.ToFieldId(v => v.Wage.Duration),
                vacancy,
                (v) =>
                {
                    return v.Wage.Duration = int.TryParse(m.Duration, out int duration) ? duration : default(int?);
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.DurationUnit,
                FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit),
                vacancy,
                (v) =>
                {
                    return v.Wage.DurationUnit = m.DurationUnit;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WorkingWeekDescription,
                FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription),
                vacancy,
                (v) =>
                {
                    return v.Wage.WorkingWeekDescription = m.WorkingWeekDescription;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.Wage.WeeklyHours,
                FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours),
                vacancy,
                (v) =>
                {
                    return v.Wage.WeeklyHours = m.WeeklyHours.AsDecimal(2);
                });

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, DurationEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, DurationEditModel>();

            mappings.Add(e => e.Wage.Duration, vm => vm.Duration);
            mappings.Add(e => e.Wage.DurationUnit, vm => vm.DurationUnit);
            mappings.Add(e => e.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription);
            mappings.Add(e => e.Wage.WeeklyHours, vm => vm.WeeklyHours);

            return mappings;
        }
    }
}