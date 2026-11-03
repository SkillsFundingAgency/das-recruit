using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Interfaces;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2;

public class AdditionalQuestionsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<AdditionalQuestionsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IOptions<ExternalLinksConfiguration> options,
    IUtility utility)
    : VacancyValidatingOrchestrator<AdditionalQuestionsEditModel>(logger), IAdditionalQuestionsOrchestrator
{
    public async Task<AdditionalQuestionsViewModel> GetViewModel(VacancyRouteModel routeModel)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.AdditionalQuestions_Get);

        var viewModel = new AdditionalQuestionsViewModel
        {
            VacancyId = vacancy.Id,
            Ukprn = routeModel.Ukprn,
            AdditionalQuestion1 = vacancy.AdditionalQuestion1,
            AdditionalQuestion2 = vacancy.AdditionalQuestion2,
            FindAnApprenticeshipUrl = options.Value.FindAnApprenticeshipUrl,
            VacancyTitle = vacancy.Title,
            ApprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault(),
            QuestionCount = vacancy.ApprenticeshipType.GetValueOrDefault() == ApprenticeshipTypes.Foundation ? 3 : 4,
        };
            
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, 
                ReviewFieldMappingLookups.GetAdditionalQuestionsFieldIndicators());
        }
            
        viewModel.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);

        return viewModel;
    }
    
    public async Task<OrchestratorResponse> PostEditModel(AdditionalQuestionsEditModel editModel, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.AdditionalQuestions_Post);
        
        vacancy.HasSubmittedAdditionalQuestions = true;

        editModel.QuestionCount = vacancy.ApprenticeshipType.GetValueOrDefault() == ApprenticeshipTypes.Foundation ? 3 : 4;

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.AdditionalQuestion1,
            FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1),
            vacancy,
            (v) => { return v.AdditionalQuestion1 = editModel.AdditionalQuestion1; });
        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.AdditionalQuestion2,
            FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2),
            vacancy,
            (v) => { return v.AdditionalQuestion2 = editModel.AdditionalQuestion2; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, VacancyRuleSet.AdditionalQuestion1 | VacancyRuleSet.AdditionalQuestion2),
            v => vacancyClient.UpdateDraftVacancyAsync(v, user));
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, AdditionalQuestionsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, AdditionalQuestionsEditModel>
        {
            { e => e.AdditionalQuestion1, vm => vm.AdditionalQuestion1 },
            { e => e.AdditionalQuestion2, vm => vm.AdditionalQuestion2 }
        };

        return mappings;
    }
}