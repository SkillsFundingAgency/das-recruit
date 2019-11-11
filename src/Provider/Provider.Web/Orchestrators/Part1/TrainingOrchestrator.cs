using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1 
{
    public class TrainingOrchestrator : EntityValidatingOrchestrator<Vacancy, TrainingEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TrainingOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TrainingOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(
                _client, _vacancyClient, vrm, RouteNames.Training_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;
            
            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id,
                SelectedProgrammeId = vacancy.ProgrammeId,
                Programmes = programmes.ToViewModel(),
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<TrainingViewModel> GetTrainingViewModelAsync(TrainingEditModel m, VacancyUser user)
        {
            var vm = await GetTrainingViewModelAsync((VacancyRouteModel)m, user);

            vm.SelectedProgrammeId = m.SelectedProgrammeId;

            return vm;
        }

        public async Task<ConfirmTrainingViewModel> GetConfirmTrainingViewModelAsync(VacancyRouteModel vrm, string programmeId)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Training_Confirm_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == programmeId);

            if (programme == null)
                return null;

            return new ConfirmTrainingViewModel
            {
                ProgrammeId = programme.Id,
                ApprenticeshipLevel = programme.ApprenticeshipLevel,
                TrainingTitle = programme.Title,
                DurationMonths = programme.Duration,
                ProgrammeType = programme.ApprenticeshipType.GetDisplayName(),
                PageInfo = Utility.GetPartOnePageInfo(vacancyTask.Result),
                TrainingEffectiveToDate = programme.EffectiveTo?.AsGdsDate(),
                EducationLevelName =
                    EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel)
            };
        }

        public async Task<OrchestratorResponse> PostConfirmTrainingEditModelAsync(ConfirmTrainingEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Training_Confirm_Post);

            vacancy.ProgrammeId = m.ProgrammeId;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public async Task<IApprenticeshipProgramme> GetProgrammeAsync(string programmeId)
        {
            var programmes = await _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            return programmes.SingleOrDefault(p => p.Id == programmeId);
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel>();

            mappings.Add(e => e.ProgrammeId, vm => vm.SelectedProgrammeId);

            return mappings;
        }
    }
}