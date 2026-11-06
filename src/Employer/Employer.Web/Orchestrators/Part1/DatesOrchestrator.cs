using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public class DatesOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<DatesOrchestrator> logger,
    ITimeProvider timeProvider,
    IReviewSummaryService reviewSummaryService,
    IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
    IUtility utility)
    : VacancyValidatingOrchestrator<DatesEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate;

    public async Task<DatesViewModel> GetDatesViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
            
        var vm = new DatesViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vrm.EmployerAccountId,
            IsDisabilityConfident = vacancy.IsDisabilityConfident,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            CurrentYear = timeProvider.Now.Year,
            Title = vacancy.Title
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
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetDatesReviewFieldIndicators());
        }
        if (vacancy.StartDate.HasValue && vacancy.ClosingDate.HasValue)
            vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
        return vm;
    }

    private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
    {
        var result = vacancyClient.Validate(vacancy, ValidationRules);
        MapValidationPropertiesToViewModel(result);
        return result;
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

        return vm;
    }

    public async Task<OrchestratorResponse> PostDatesEditModelAsync(DatesEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.ClosingDate,
            FieldIdResolver.ToFieldId(v => v.ClosingDate),
            vacancy,
            (v) =>
            {
                return v.ClosingDate = m.ClosingDate.AsDateTimeUk()?.ToUniversalTime();
            });

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.StartDate,
            FieldIdResolver.ToFieldId(v => v.StartDate),
            vacancy,
            (v) =>
            {
                return v.StartDate = m.StartDate.AsDateTimeUk()?.ToUniversalTime();
            });

            

        return await ValidateAndExecute(
            vacancy, 
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    private async Task SetTrainingProgrammeAsync(Vacancy vacancy, DatesViewModel vm)
    {
        if (string.IsNullOrEmpty(vacancy.ProgrammeId))
            return;

        var programme = await apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

        if (programme?.EffectiveTo.HasValue == true)
        {
            vm.TrainingDescription = $"{programme.Title}, Level:{(int)programme.ApprenticeshipLevel}";
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
}