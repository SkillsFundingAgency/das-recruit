using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class RecruitNationallyController: Controller
{
    private static readonly Dictionary<string, string> ValidationFieldMappings = new()
    {
        { "EmployerLocationInformation", "AdditionalInformation" }
    };
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("location-information", Name = RouteNames.RecruitNationally_Get)]
    public async Task<IActionResult> RecruitNationally([FromServices] IUtility utility, [FromServices] IReviewSummaryService reviewSummaryService, VacancyRouteModel model, [FromQuery] bool wizard = true)
    {
        ModelState.ThrowIfBindingErrors();
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Get);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            Ukprn = model.Ukprn,
            AdditionalInformation = vacancy.EmployerLocationInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }

        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-information", Name = RouteNames.RecruitNationally_Post)]
    public async Task<IActionResult> RecruitNationally(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        RecruitNationallyEditModel model,
        [FromQuery] bool wizard = true)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Post);
        var result = await vacancyLocationService.UpdateDraftVacancyLocations(
            vacancy,
            User.ToVacancyUser(),
            AvailableWhere.AcrossEngland,
            null,
            model.AdditionalInformation);

        if (result.ValidationResult is null)
        {
            return utility.IsTaskListCompleted(vacancy)
                ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { model.VacancyId, model.Ukprn, wizard })
                : RedirectToRoute(RouteNames.ProviderTaskListGet, new { model.VacancyId, model.Ukprn, wizard });
        }
        
        ModelState.AddValidationErrorsWithFieldMappings(result.ValidationResult, ValidationFieldMappings);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            Ukprn = model.Ukprn,
            AdditionalInformation = model.AdditionalInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }
        
        return View(viewModel);
    }
}