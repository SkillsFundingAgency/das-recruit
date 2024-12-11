using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Domain;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class MultipleLocationsController : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("location-availability", Name = RouteNames.MultipleLocations_Get)]
    public async Task<IActionResult> LocationAvailability(
        [FromServices] IUtility utility,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard = true)
    {
        var viewModel = await GetLocationAvailabilityViewModel(utility, vacancyRouteModel, null, wizard);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-availability", Name = RouteNames.MultipleLocations_Post)]
    public async Task<IActionResult> LocationAvailability(
        [FromServices] IUtility utility,
        LocationAvailabilityEditModel model,
        [FromQuery] bool wizard)
    {
        if (ModelState.IsValid)
        {
            return model.SelectedAvailability switch
            {
                AvailableWhere.OneLocation => throw new NotImplementedException(),
                AvailableWhere.MultipleLocations => RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, wizard }), 
                AvailableWhere.AcrossEngland => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        var viewModel = await GetLocationAvailabilityViewModel(utility, model, model.SelectedAvailability, wizard);
        return View(viewModel);
    }

    private static async Task<LocationAvailabilityViewModel> GetLocationAvailabilityViewModel(IUtility utility, VacancyRouteModel vacancyRouteModel, AvailableWhere? availableWhere, bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.MultipleLocations_Get);
        var viewModel = new LocationAvailabilityViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = vacancyRouteModel.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedAvailability = availableWhere ?? vacancy.EmployerLocationOption ?? (vacancy.EmployerLocation is not null ? AvailableWhere.OneLocation : null),
            VacancyId = vacancyRouteModel.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return viewModel;
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("add-many-locations", Name = RouteNames.AddMoreThanOneLocation_Get)]
    public async Task<IActionResult> AddMoreThanOneLocation(
        [FromServices] IMultipleLocationsOrchestrator orchestrator,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard)
    {
        var viewModel = await orchestrator.GetAddMoreThanOneLocationViewModelAsync(vacancyRouteModel, wizard);

        // Override with stored values after a round trip through add new location
        if (TempData[TempDataKeys.SelectedLocations] is string value)
        {
            viewModel.SelectedLocations = JsonSerializer.Deserialize<List<string>>(value);
        }
        
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-many-locations", Name = RouteNames.AddMoreThanOneLocation_Post)]
    public async Task<IActionResult> AddMoreThanOneLocation(
        [FromServices] IMultipleLocationsOrchestrator orchestrator,
        AddMoreThanOneLocationEditModel editModel,
        [FromQuery] bool wizard)
    {
        var result = await orchestrator.PostAddMoreThanOneLocationViewModelAsync(editModel, User.ToVacancyUser());

        if (result.Success)
        {
            return RedirectToRoute(RouteNames.MultipleLocationsConfirm_Get, new { editModel.VacancyId, editModel.EmployerAccountId, wizard } );
        }
        
        result.AddErrorsToModelState(ModelState);
        var viewModel = await orchestrator.GetAddMoreThanOneLocationViewModelAsync(editModel, wizard);
        viewModel.SelectedLocations = editModel.SelectedLocations;
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("addnewlocation", Name = RouteNames.AddNewLocationJourney_Post)]
    public IActionResult AddALocation(AddMoreThanOneLocationEditModel editModel, [FromQuery] bool wizard)
    {
        TempData[TempDataKeys.SelectedLocations] = JsonSerializer.Serialize(editModel.SelectedLocations);
        return RedirectToRoute(RouteNames.AddLocation_Get, new { editModel.VacancyId, editModel.EmployerAccountId, wizard, origin = MultipleLocationsJourneyOrigin.Many } );
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("confirm-locations", Name = RouteNames.MultipleLocationsConfirm_Get)]
    public async Task<IActionResult> ConfirmLocations(
        [FromServices] IUtility utility,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.MultipleLocations_Get);
        var viewModel = new ConfirmLocationsViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = vacancyRouteModel.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            Locations = vacancy.EmployerLocations,
            VacancyId = vacancyRouteModel.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("confirm-locations", Name = RouteNames.MultipleLocationsConfirm_Post)]
    public IActionResult ConfirmLocations(VacancyRouteModel vacancyRouteModel, [FromQuery] bool wizard)
    {
        return wizard
            ? RedirectToRoute(RouteNames.EmployerTaskListGet, new {vacancyRouteModel.VacancyId, vacancyRouteModel.EmployerAccountId, wizard}) 
            : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vacancyRouteModel.VacancyId, vacancyRouteModel.EmployerAccountId});
    }
}