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
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1 
{
    public class TrainingOrchestrator(
        IRecruitVacancyClient vacancyClient,
        IProviderVacancyClient providerVacancyClient,
        ILogger<TrainingOrchestrator> logger,
        IReviewSummaryService reviewSummaryService,
        IUtility utility)
        : VacancyValidatingOrchestrator<TrainingEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
        private const string InvalidTraining = "Select a valid training course";

        public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_Get);
            var programmesTask = providerVacancyClient.GetActiveApprenticeshipProgrammesAsync((int)vrm.Ukprn);

            await Task.WhenAll(vacancyTask, programmesTask);

            var employerInfo =
                await providerVacancyClient.GetProviderEmployerVacancyDataAsync(vrm.Ukprn,
                    vacancyTask.Result.EmployerAccountId);

            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result;
            
            var vm = new TrainingViewModel
            {
                Title = vacancy.Title,
                VacancyId = vacancy.Id,
                SelectedProgrammeId = vacancy.ProgrammeId,
                Programmes = programmes.ToViewModel(),
                PageInfo = utility.GetPartOnePageInfo(vacancy),
                HasMoreThanOneLegalEntity = employerInfo.LegalEntities.Count > 1,
                Ukprn = vrm.Ukprn,
                IsTaskListCompleted = utility.IsTaskListCompleted(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
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
            var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_Confirm_Get);
            var programmesTask = vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);
            var vacancy = vacancyTask.Result;
            var programmes = programmesTask.Result.ToList();

            var programme = programmes.SingleOrDefault(p => p.Id == programmeId);
            if (programme == null)
                return null;

            return new ConfirmTrainingViewModel
            {
                Title = vacancyTask.Result.Title,
                ProgrammeId = programme.Id,
                ApprenticeshipLevel = programme.ApprenticeshipLevel,
                TrainingTitle = programme.Title,
                DurationMonths = programme.Duration,
                ProgrammeType = programme.ApprenticeshipType.GetDisplayName(),
                PageInfo = utility.GetPartOnePageInfo(vacancyTask.Result),
                TrainingEffectiveToDate = programme.EffectiveTo?.AsGdsDate(),
                EducationLevelName = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel),
                IsFoundation = programme.ApprenticeshipType == TrainingType.Foundation,
                IsChangingApprenticeshipType = vacancy.IsChangingApprenticeshipType(programmes, programme),
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };
        }

        public async Task<OrchestratorResponse> PostConfirmTrainingEditModelAsync(ConfirmTrainingEditModel m, VacancyUser user)
        {
            var programmes = (await vacancyClient.GetActiveApprenticeshipProgrammesAsync()).ToList();
            var programme = programmes.SingleOrDefault(p => p.Id == m.ProgrammeId);
            if (programme == null)
            {
                return new OrchestratorResponse(new EntityValidationResult
                {
                    Errors = [new EntityValidationError(0, nameof(TrainingEditModel.SelectedProgrammeId), InvalidTraining, string.Empty)]
                });
            }
            
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.Training_Confirm_Post);
            vacancy.ApprenticeshipType = programme.ApprenticeshipType switch {
                TrainingType.Foundation => ApprenticeshipTypes.Foundation,
                _ => null
            };
            
            if (vacancy.IsChangingApprenticeshipType(programmes, programme))
            {
                ProcessApprenticeshipTypeChanges(vacancy, programme);
            }

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ProgrammeId,
                FieldIdResolver.ToFieldId(v => v.ProgrammeId),
                vacancy,
                (v) =>
                {
                    return v.ProgrammeId = m.ProgrammeId;
                });

            return await ValidateAndExecute(
                vacancy,
                v => vacancyClient.Validate(v, ValidationRules),
                v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }
        
        private static void ProcessApprenticeshipTypeChanges(Vacancy vacancy, IApprenticeshipProgramme programme)
        {
            switch (programme.ApprenticeshipType)
            {
                case TrainingType.Foundation:
                    vacancy.Skills = null;
                    vacancy.Qualifications = null;
                    vacancy.HasOptedToAddQualifications = null;
                    break;
            }
        }

        public async Task<IApprenticeshipProgramme> GetProgrammeAsync(string programmeId)
        {
            var programmes = await vacancyClient.GetActiveApprenticeshipProgrammesAsync();

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