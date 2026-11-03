using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2;

public class FutureProspectsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<FutureProspectsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<FutureProspectsEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.OutcomeDescription;

    public async Task<FutureProspectsViewModel> GetFutureProspectsViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy =
            await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.FutureProspects_Get);

        var vm = new FutureProspectsViewModel
        {
            Title = vacancy.Title,
            FutureProspects = vacancy.OutcomeDescription,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators());
        }

        vm.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);

        return vm;
    }

    public async Task<FutureProspectsViewModel> GetFutureProspectsViewModelAsync(FutureProspectsEditModel m)
    {
        var vm = await GetFutureProspectsViewModelAsync((VacancyRouteModel)m);

        vm.FutureProspects = m.FutureProspects;

        return vm;
    }

    public async Task<OrchestratorResponse> PostFutureProspectsEditModelAsync(FutureProspectsEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.FutureProspects_Post);

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.OutcomeDescription,
            FieldIdResolver.ToFieldId(v => v.OutcomeDescription),
            vacancy,
            (v) => { return v.OutcomeDescription = m.FutureProspects; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel>();

        mappings.Add(e => e.OutcomeDescription, vm => vm.FutureProspects);

        return mappings;
    }
}