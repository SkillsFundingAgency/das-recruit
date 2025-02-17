using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class MultipleLocationsController(
    IWebHostEnvironment hostingEnvironment,
    IUtility utility) : EmployerControllerBase(hostingEnvironment)
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("location-availability", Name = RouteNames.MultipleLocations_Get)]
    public async Task<IActionResult> LocationAvailability(VacancyRouteModel vacancyRouteModel, [FromQuery] string wizard = "true")
    {
        var viewModel = await GetLocationAvailabilityViewModel(vacancyRouteModel, wizard);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-availability", Name = RouteNames.MultipleLocations_Post)]
    public async Task<IActionResult> LocationAvailability(LocationAvailabilityEditModel model, [FromQuery] bool wizard)
    {
        // TODO: validate the model
        // TODO: save the selection
        
        return RedirectToRoute(RouteNames.ProviderTaskListGet, new { Wizard = wizard, model.Ukprn, model.VacancyId });
    } 

    private async Task<LocationAvailabilityViewModel> GetLocationAvailabilityViewModel(VacancyRouteModel vacancyRouteModel, string wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Location_Get);
        var viewModel = new LocationAvailabilityViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            Ukprn = vacancyRouteModel.Ukprn,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedAvailability = vacancy.EmployerLocation is not null ? AvailableWhere.OneLocation : null,
            VacancyId = vacancyRouteModel.VacancyId,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy)
        };
        viewModel.PageInfo.SetWizard(wizard);
        
        return viewModel;
    }
}