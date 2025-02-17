using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Domain;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class AddLocationController(IUtility utility) : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("add-location", Name = RouteNames.AddLocation_Get)]
    public async Task<IActionResult> AddLocation(VacancyRouteModel vacancyRouteModel, [FromQuery] MultipleLocationsJourneyOrigin origin, [FromQuery] bool wizard = true)
    {
        var model = await GetAddLocationViewModel(vacancyRouteModel, null, origin, wizard);
        return View(model);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-location", Name = RouteNames.AddLocation_Post)]
    public async Task<IActionResult> AddLocation(
        [FromServices] IValidator<AddLocationEditModel> validator, 
        AddLocationEditModel model, 
        [FromQuery] MultipleLocationsJourneyOrigin origin, 
        [FromQuery] bool wizard = true,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(model, cancellationToken);

        if (validationResult.IsValid)
        {
            return RedirectToRoute(RouteNames.SelectAnAddress_Get, new { model.VacancyId, model.EmployerAccountId, wizard, origin, model.Postcode });    
        }
        
        ModelState.AddModelError(nameof(AddLocationEditModel.Postcode), validationResult.ToString());
        var viewModel = await GetAddLocationViewModel(model, model.Postcode, origin, wizard);
        return View(viewModel);
    }
    
    private async Task<AddLocationViewModel> GetAddLocationViewModel(VacancyRouteModel vacancyRouteModel, string postCode, MultipleLocationsJourneyOrigin origin, bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Location_Get);
        var viewModel = new AddLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = vacancyRouteModel.EmployerAccountId,
            Origin = origin,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            Postcode = postCode,
            VacancyId = vacancyRouteModel.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return viewModel;
    }
}