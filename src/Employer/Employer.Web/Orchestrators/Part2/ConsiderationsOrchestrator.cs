using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class ConsiderationsOrchestrator(
    ILogger<ConsiderationsOrchestrator> logger,
    IRecruitVacancyClient vacancyClient,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<ConsiderationsEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;

    public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
            
        var vm = new ConsiderationsViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            ThingsToConsider = vacancy.ThingsToConsider,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy)
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetConsiderationsFieldIndicators());
        }
            
        return vm;
    }

    public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(ConsiderationsEditModel m)
    {
        var vm = await GetConsiderationsViewModelAsync((VacancyRouteModel)m);

        vm.ThingsToConsider = m.ThingsToConsider;

        return vm;
    }

    public async Task<OrchestratorResponse> PostConsiderationsEditModelAsync(ConsiderationsEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.ThingsToConsider,
            FieldIdResolver.ToFieldId(v => v.ThingsToConsider),
            vacancy,
            (v) => { return v.ThingsToConsider = m.ThingsToConsider; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel>();

        mappings.Add(e => e.ThingsToConsider, vm => vm.ThingsToConsider);

        return mappings;
    }
}