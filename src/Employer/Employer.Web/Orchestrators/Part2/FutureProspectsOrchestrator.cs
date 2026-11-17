using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class FutureProspectsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<FutureProspectsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<FutureProspectsEditModel>(logger), IFutureProspectsOrchestrator
{
    public async Task<FutureProspectsViewModel> GetViewModel(VacancyRouteModel routeModel)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(routeModel);

        var viewModel = new FutureProspectsViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            FutureProspects = vacancy.OutcomeDescription
        };
            
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators());
        }
            
        viewModel.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);

        return viewModel;
    }

    public async Task<OrchestratorResponse> PostEditModel(FutureProspectsEditModel editModel, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(editModel);
            
        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.OutcomeDescription,
            FieldIdResolver.ToFieldId(v => v.OutcomeDescription),
            vacancy,
            (v) => { return v.OutcomeDescription = editModel.FutureProspects; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, VacancyRuleSet.OutcomeDescription),
            v => vacancyClient.UpdateDraftVacancyAsync(v, user));
    }
        
    protected override EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel>();

        mappings.Add(e => e.OutcomeDescription, vm => vm.FutureProspects);

        return mappings;
    }
}