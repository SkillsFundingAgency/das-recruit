using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TrainingOrchestrator : VacancyValidatingOrchestrator<TrainingEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public TrainingOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TrainingOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility, IEmployerVacancyClient employerVacancyClient) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
            _employerVacancyClient = employerVacancyClient;
        }
        
        public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            var isUsersFirstVacancyTask = IsUsersFirstVacancy(user.UserId);
            var getEmployerDataTask = _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);

            await Task.WhenAll(vacancyTask, programmesTask, isUsersFirstVacancyTask, getEmployerDataTask);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;

            var vm = new TrainingViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vrm.EmployerAccountId,
                SelectedProgrammeId = vacancy.ProgrammeId,
                Programmes = programmes.ToViewModel(),
                IsUsersFirstVacancy = isUsersFirstVacancyTask.Result && vacancy.TrainingProvider == null,
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                HasMoreThanOneLegalEntity = getEmployerDataTask.Result.LegalEntities.Count() > 1
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

        public async Task<TrainingFirstVacancyViewModel> GetTrainingFirstVacancyViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_First_Time_Get);

            return new TrainingFirstVacancyViewModel();
        }

        public async Task<ConfirmTrainingViewModel> GetConfirmTrainingViewModelAsync(VacancyRouteModel vrm, string programmeId)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_Confirm_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == programmeId);

            if (programme == null)
                return null;

            return new ConfirmTrainingViewModel
            {
                VacancyId = vrm.VacancyId,
                EmployerAccountId = vrm.EmployerAccountId,
                ProgrammeId = programme.Id,
                ApprenticeshipLevel = programme.ApprenticeshipLevel,
                TrainingTitle = programme.Title,
                DurationMonths = programme.Duration,
                ProgrammeType = programme.ApprenticeshipType.GetDisplayName(),
                PageInfo = _utility.GetPartOnePageInfo(vacancyTask.Result),
                TrainingEffectiveToDate = programme.EffectiveTo?.AsGdsDate(),
                EducationLevelName =
                    EducationLevelNumberHelper.GetTableFormatEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel),
                IsFoundation = programme.ApprenticeshipType == TrainingType.Foundation,
            };
        }

        public async Task<OrchestratorResponse> PostConfirmTrainingEditModelAsync(ConfirmTrainingEditModel m,
            IApprenticeshipProgramme programme, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Training_Confirm_Post);
            vacancy.ApprenticeshipType = programme.ApprenticeshipType switch {
                TrainingType.Foundation => ApprenticeshipTypes.Foundation,
                _ => null
            };
            
            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ProgrammeId,
                FieldIdResolver.ToFieldId(v => v.ProgrammeId),
                vacancy,
                (v) =>
                {
                    return v.ProgrammeId = m.ProgrammeId;
                });

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

        private async Task<bool> IsUsersFirstVacancy(string userId)
        {
            var userVacancies = await _client.GetVacancyCountForUserAsync(userId);

            return userVacancies <= 1;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel>();

            mappings.Add(e => e.ProgrammeId, vm => vm.SelectedProgrammeId);

            return mappings;
        }
    }
}
