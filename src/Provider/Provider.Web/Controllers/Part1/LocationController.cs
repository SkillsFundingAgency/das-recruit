using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class LocationController(IWebHostEnvironment hostingEnvironment) : EmployerControllerBase(hostingEnvironment)
{
    #region When FeatureNames.MultipleLocations feature flag is removed, all this can be removed

    [HttpGet("location", Name = RouteNames.Location_Get)]
    public async Task<IActionResult> Location([FromServices] LocationOrchestrator orchestrator, VacancyRouteModel vrm, [FromQuery] string wizard = "true")
    {
        var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());
        var vm = await orchestrator.GetLocationViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            
        vm.PageInfo.SetWizard(wizard);
            
        //back link is available only if cookie is not there (back link in part 1)
        //or part 2 has not started (coming from preview)
        vm.CanShowBackLink = employerInfoModel != null || vm.PageInfo.IsWizard;

        //if cookie is missing and user is in part1 then create the cookie to support back navigation
        //either part 1 is not completed or part 1 is completed but part 2 has not started
        if (employerInfoModel == null && (!vm.PageInfo.HasCompletedPartOne || !vm.PageInfo.HasStartedPartTwo))
        {
            employerInfoModel = await orchestrator.GetVacancyEmployerInfoModelAsync(vrm);
            SetVacancyEmployerInfoCookie(employerInfoModel);
        }
        return View(vm);
    }

    [HttpPost("location", Name = RouteNames.Location_Post)]
    public async Task<IActionResult> Location([FromServices] LocationOrchestrator orchestrator, LocationEditModel model, [FromQuery] bool wizard)
    {
        var employerInfoModel = GetVacancyEmployerInfoCookie(model.VacancyId.GetValueOrDefault());
        var response = await orchestrator.PostLocationEditModelAsync(model, User.ToVacancyUser(), User.GetUkprn(), employerInfoModel);

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetLocationViewModelAsync(model, employerInfoModel, User.ToVacancyUser());
            vm.SelectedLocation = model.SelectedLocation;
            vm.PageInfo.SetWizard(wizard);
            vm.CanShowBackLink = employerInfoModel != null || vm.PageInfo.IsWizard;
            vm.AddressLine1 = model.AddressLine1;
            vm.AddressLine2 = model.AddressLine2;
            vm.AddressLine3 = model.AddressLine3;
            vm.AddressLine4 = model.AddressLine4;
            vm.Postcode = model.Postcode;
            return View(vm);
        }

        DeleteVacancyEmployerInfoCookie();

        return RedirectToRoute(RouteNames.ProviderTaskListGet, new { Wizard = wizard, model.Ukprn, model.VacancyId });
    }

    [HttpGet("location-cancel", Name = RouteNames.Location_Cancel)]
    public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
    {
        return CancelAndRedirect(wizard, vrm);
    }

    [HttpGet("location/GetAddresses")]
    public async Task<IActionResult> GetAddresses([FromServices] LocationOrchestrator orchestrator, [FromQuery] string searchTerm)
    {
        var result = await orchestrator.GetAddresses(searchTerm);
        return Ok(result);
    }
    
    #endregion
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("add-one-location", Name = RouteNames.AddOneLocation_Get)]
    public async Task<IActionResult> AddOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.AddOneLocation_Get);
        var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy, vacancyRouteModel.Ukprn);
        var selectedLocation = vacancy.EmployerLocations is { Count: 1 } ? vacancy.EmployerLocations[0] : null;

        var viewModel = new AddOneLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            AvailableLocations = allLocations ?? [],
            VacancyId = vacancyRouteModel.VacancyId,
            Ukprn = vacancyRouteModel.Ukprn,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedLocation = selectedLocation?.ToAddressString(),
        };
        viewModel.PageInfo.SetWizard(wizard);
        
        if (TempData[TempDataKeys.AddedLocation] is string newlyAddedLocation)
        {
            viewModel.SelectedLocation = newlyAddedLocation;
            viewModel.BannerAddress = newlyAddedLocation;
        }
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-one-location", Name = RouteNames.AddOneLocation_Post)]
    public async Task<IActionResult> AddOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        AddOneLocationEditModel model,
        [FromQuery] bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddOneLocation_Post);
        var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy, model.Ukprn);
        var location = allLocations.FirstOrDefault(x => x.ToAddressString() == model.SelectedLocation);
        var result = await vacancyLocationService.UpdateDraftVacancyLocations(
            vacancy,
            User.ToVacancyUser(),
            AvailableWhere.OneLocation,
            location is null ? null : [location]);

        if (result.ValidationResult is null)
        {
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new { Wizard = wizard, model.Ukprn, model.VacancyId });
        }

        ModelState.AddValidationErrors(result.ValidationResult, new Dictionary<string, string> { { "EmployerLocations", "SelectedLocation" } });
        var viewModel = new AddOneLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            AvailableLocations = allLocations,
            VacancyId = model.VacancyId,
            Ukprn = model.Ukprn,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedLocation = model.SelectedLocation
        };
        viewModel.PageInfo.SetWizard(wizard);
        return View(viewModel);
    }
}