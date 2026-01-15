using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class VacancyPreviewController(VacancyPreviewOrchestrator orchestrator) : Controller
{
    [HttpGet("advert-preview", Name = RouteNames.Vacancy_Advert_Preview_Get)]
    public async Task<IActionResult> AdvertPreview(VacancyRouteModel vrm)
    {
        var viewModel = await orchestrator.GetVacancyPreviewViewModelAsync(vrm);

        if (TempData.TryGetValue(TempDataKeys.VacancyPreviewInfoMessage, out var value))
        {
            viewModel.InfoMessage = value.ToString();
        }

        AddSoftValidationErrorsToModelState(viewModel);
        viewModel.CanHideValidationSummary = true;
        return View(ViewNames.AdvertPreview , viewModel);
    }

    private void AddSoftValidationErrorsToModelState(VacancyPreviewViewModel viewModel)
    {
        if (!viewModel.SoftValidationErrors.HasErrors)
            return;

        foreach (var error in viewModel.SoftValidationErrors.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}