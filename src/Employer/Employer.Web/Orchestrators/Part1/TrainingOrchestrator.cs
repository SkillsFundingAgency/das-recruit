using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public class TrainingOrchestrator(
    IEmployerVacancyClient client,
    IRecruitVacancyClient vacancyClient,
    ILogger<TrainingOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility,
    IEmployerVacancyClient employerVacancyClient)
    : VacancyValidatingOrchestrator<TrainingEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
    private const string InvalidTraining = "Select the training the apprentice will take";

    public async Task<TrainingViewModel> GetTrainingViewModelAsync(VacancyRouteModel vrm, VacancyUser user)
    {
        var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_Get);
        var programmesTask = vacancyClient.GetActiveApprenticeshipProgrammesAsync();
        var isUsersFirstVacancyTask = IsUsersFirstVacancy(user.UserId);
        var getEmployerDataTask = employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);

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
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            HasMoreThanOneLegalEntity = getEmployerDataTask.Result.LegalEntities.Count() > 1
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

    public async Task<TrainingFirstVacancyViewModel> GetTrainingFirstVacancyViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Training_First_Time_Get);

        return new TrainingFirstVacancyViewModel();
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

        var result = new ConfirmTrainingViewModel
        {
            VacancyId = vrm.VacancyId,
            EmployerAccountId = vrm.EmployerAccountId,
            ProgrammeId = programme.Id,
            ApprenticeshipLevel = programme.ApprenticeshipLevel,
            TrainingTitle = programme.Title,
            DurationMonths = programme.Duration,
            ProgrammeType = programme.ApprenticeshipType.GetDisplayName(),
            PageInfo = utility.GetPartOnePageInfo(vacancyTask.Result),
            TrainingEffectiveToDate = programme.EffectiveTo?.AsGdsDate(),
            EducationLevelName = EducationLevelNumberHelper.GetTableFormatEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel),
            IsFoundation = programme.ApprenticeshipType == TrainingType.Foundation,
            IsChangingApprenticeshipType = vacancy.IsChangingApprenticeshipType(programmes, programme),
            WillTaskListBeCompleted = utility.IsTaskListCompleted(VacancyWithProposedChanges(vacancy, programme)),
        };

        return result;
    }

    private static Vacancy VacancyWithProposedChanges(Vacancy vacancy, IApprenticeshipProgramme programme)
    {
        // we need to set the vacancy up as if it had the proposed changes so that the tasklist validator can work correctly
        // clone the vacancy to make sure nothing else gets affected
        var clone = JsonConvert.DeserializeObject<Vacancy>(JsonConvert.SerializeObject(vacancy));
        clone.ApprenticeshipType = programme.ApprenticeshipType switch
        {
            TrainingType.Foundation => ApprenticeshipTypes.Foundation,
            _ => null
        };
        return clone;
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

    private async Task<bool> IsUsersFirstVacancy(string userId)
    {
        int userVacancies = await client.GetVacancyCountForUserAsync(userId);
        return userVacancies <= 1;
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, TrainingEditModel>();

        mappings.Add(e => e.ProgrammeId, vm => vm.SelectedProgrammeId);

        return mappings;
    }
}