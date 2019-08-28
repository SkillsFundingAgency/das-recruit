using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class DurationOrchestrator : EntityValidatingOrchestrator<Vacancy, DurationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Duration | VacancyRuleSet.WorkingWeekDescription | VacancyRuleSet.WeeklyHours;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public DurationOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<DurationOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<DurationViewModel> GetDurationViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Duration_Get);

            var training = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var vm = new DurationViewModel
            {
                Duration = vacancy.Wage?.Duration?.ToString(),
                DurationUnit = vacancy.Wage?.DurationUnit ?? DurationUnit.Month,
                WorkingWeekDescription = vacancy.Wage?.WorkingWeekDescription,
                WeeklyHours = $"{vacancy.Wage?.WeeklyHours:0.##}",
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                TrainingTitle = training?.Title,
                TrainingDurationMonths = training?.Duration ?? 0
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
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Duration_Post);

            if(vacancy.Wage == null)
                vacancy.Wage = new Wage();

            vacancy.Wage.Duration = int.TryParse(m.Duration, out int duration) ? duration : default(int?);
            vacancy.Wage.DurationUnit = m.DurationUnit;
            vacancy.Wage.WorkingWeekDescription = m.WorkingWeekDescription;
            vacancy.Wage.WeeklyHours = m.WeeklyHours.AsDecimal(2);
            
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
