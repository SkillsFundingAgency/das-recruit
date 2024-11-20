using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class MultipleLocationsController(
    IUtility utility) : Controller
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
        
        return wizard 
            ? RedirectToRoute(RouteNames.EmployerTaskListGet, new {model.VacancyId, model.EmployerAccountId, wizard}) 
            : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {model.VacancyId, model.EmployerAccountId});
    } 

    public async Task<LocationAvailabilityViewModel> GetLocationAvailabilityViewModel(VacancyRouteModel vacancyRouteModel, string wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Location_Get);
        var viewModel = new LocationAvailabilityViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = vacancyRouteModel.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedAvailability = vacancy.EmployerLocation is not null ? AvailableWhere.OneLocation : null,
            VacancyId = vacancyRouteModel.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return viewModel;
    }
    
}