using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1 
{
    public class DatesOrchestrator : EntityValidatingOrchestrator<Vacancy, DatesEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;

        public DatesOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<DatesOrchestrator> logger, ITimeProvider timeProvider, IReviewSummaryService reviewSummaryService, IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _reviewSummaryService = reviewSummaryService;
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
        }

        public async Task<DatesViewModel> GetDatesViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Dates_Get);
            
            var vm = new DatesViewModel
            {
                VacancyId = vacancy.Id,
                IsDisabilityConfident = vacancy.IsDisabilityConfident,
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                CurrentYear = _timeProvider.Now.Year
            };

            if (vacancy.ClosingDate.HasValue)
            {
                vm.ClosingDay = $"{vacancy.ClosingDate.Value.Day:00}";
                vm.ClosingMonth = $"{vacancy.ClosingDate.Value.Month:00}";
                vm.ClosingYear = $"{vacancy.ClosingDate.Value.Year}";
            }

            if (vacancy.StartDate.HasValue)
            {
                vm.StartDay = $"{vacancy.StartDate.Value.Day:00}";
                vm.StartMonth = $"{vacancy.StartDate.Value.Month:00}";
                vm.StartYear = $"{vacancy.StartDate.Value.Year}";
            }

            await SetTrainingProgrammeAsync(vacancy, vm);

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetDatesReviewFieldIndicators());
            }
            if (vacancy.StartDate.HasValue && vacancy.ClosingDate.HasValue)
                vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);

            return vm;
        }

        public async Task<DatesViewModel> GetDatesViewModelAsync(DatesEditModel m)
        {
            var vm = await GetDatesViewModelAsync((VacancyRouteModel)m);

            vm.ClosingDay = m.ClosingDay;
            vm.ClosingMonth = m.ClosingMonth;
            vm.ClosingYear = m.ClosingYear;

            vm.StartDay = m.StartDay;
            vm.StartMonth = m.StartMonth;
            vm.StartYear = m.StartYear;

            vm.IsDisabilityConfident = m.IsDisabilityConfident;

            return vm;
        }

        public async Task<OrchestratorResponse> PostDatesEditModelAsync(DatesEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _client, _vacancyClient, m, RouteNames.Dates_Post);

            vacancy.ClosingDate = m.ClosingDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.StartDate = m.StartDate.AsDateTimeUk()?.ToUniversalTime();
            
            vacancy.DisabilityConfident = m.IsDisabilityConfident ? DisabilityConfident.Yes : DisabilityConfident.No;
            
            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        private async Task SetTrainingProgrammeAsync(Vacancy vacancy, DatesViewModel vm)
        {
            if (string.IsNullOrEmpty(vacancy.ProgrammeId))
                return;

            var programme = await _apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            if (programme?.EffectiveTo.HasValue == true)
            {
                vm.TrainingDescription = $"{programme.Title}, Level:{(int) programme.ApprenticeshipLevel}";
                vm.TrainingEffectiveToDate = programme.EffectiveTo.Value.AsGdsDate();
            }
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, DatesEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, DatesEditModel>();

            mappings.Add(e => e.StartDate, vm => vm.StartDate);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);

            return mappings;
        }

        private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
        {
            var result = _vacancyClient.Validate(vacancy, ValidationRules);
            MapValidationPropertiesToViewModel(result);
            return result;
        }
    }
}