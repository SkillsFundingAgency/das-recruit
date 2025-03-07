using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class MultipleLocationsController : Controller
{
    private static readonly Dictionary<string, Tuple<string, string>> ValidationMappings = new()
    {
        { "EmployerLocations", Tuple.Create<string, string>("SelectedLocations", null) },
        { VacancyValidationErrorCodes.AddressCountryNotInEngland, Tuple.Create("SelectedLocations", "All locations must be in England. Your apprenticeship must be in England to advertise it on this service") },
    };
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("location-availability", Name = RouteNames.MultipleLocations_Get)]
    public async Task<IActionResult> LocationAvailability(
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard = true)
    {
        var viewModel = await GetLocationAvailabilityViewModel(utility, reviewSummaryService, vacancyRouteModel, null, wizard);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-availability", Name = RouteNames.MultipleLocations_Post)]
    public async Task<IActionResult> LocationAvailability(
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        LocationAvailabilityEditModel model,
        [FromQuery] bool wizard)
    {
        if (ModelState.IsValid)
        {
            return model.SelectedAvailability switch
            {
                AvailableWhere.OneLocation => RedirectToRoute(RouteNames.AddOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, wizard }),
                AvailableWhere.MultipleLocations => RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, wizard }), 
                AvailableWhere.AcrossEngland => RedirectToRoute(RouteNames.RecruitNationally_Get, new { model.VacancyId, model.EmployerAccountId, wizard }),
                _ => throw new NotImplementedException(),
            };
        }

        var viewModel = await GetLocationAvailabilityViewModel(utility, reviewSummaryService, model, model.SelectedAvailability, wizard);
        return View(viewModel);
    }

    private static async Task<LocationAvailabilityViewModel> GetLocationAvailabilityViewModel(IUtility utility, IReviewSummaryService reviewSummaryService, VacancyRouteModel vacancyRouteModel, AvailableWhere? availableWhere, bool wizard)
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
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }

        return viewModel;
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("add-many-locations", Name = RouteNames.AddMoreThanOneLocation_Get)]
    public async Task<IActionResult> AddMoreThanOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        VacancyRouteModel model,
        [FromQuery] bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddMoreThanOneLocation_Get);
        var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy);

        var selectedLocations = vacancy.EmployerLocations switch
        {
            _ when TempData[TempDataKeys.SelectedLocations] is string value => JsonSerializer.Deserialize<List<string>>(value),
            { Count: >0 } => vacancy.EmployerLocations.Select(l => l.ToAddressString()).ToList(),
            _ => []
        };
        
        var viewModel = new AddMoreThanOneLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            AvailableLocations = allLocations ?? [],
            VacancyId = model.VacancyId,
            EmployerAccountId = model.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedLocations = selectedLocations
        };
        viewModel.PageInfo.SetWizard(wizard);
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }

        if (TempData[TempDataKeys.AddedLocation] is string newlyAddedLocation)
        {
            viewModel.SelectedLocations.Add(newlyAddedLocation);
            viewModel.BannerAddress = newlyAddedLocation;
        }
        
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-many-locations", Name = RouteNames.AddMoreThanOneLocation_Post)]
    public async Task<IActionResult> AddMoreThanOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        AddMoreThanOneLocationEditModel editModel,
        [FromQuery] bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.AddMoreThanOneLocation_Post);
        var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy);
        var locations = editModel.SelectedLocations
            .Select(x => allLocations.FirstOrDefault(l => l.ToAddressString() == x))
            .Where(x => x is not null)
            .ToList();
        var result = await vacancyLocationService.UpdateDraftVacancyLocations(
            vacancy,
            User.ToVacancyUser(),
            AvailableWhere.MultipleLocations,
            locations);

        if (result.ValidationResult is null)
        {
            return RedirectToRoute(RouteNames.MultipleLocationsConfirm_Get, new { editModel.VacancyId, editModel.EmployerAccountId, wizard } );
        }

        ModelState.AddValidationErrorsWithMappings(result.ValidationResult, ValidationMappings);
        var viewModel = new AddMoreThanOneLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            AvailableLocations = allLocations ?? [],
            VacancyId = editModel.VacancyId,
            EmployerAccountId = editModel.EmployerAccountId,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedLocations = editModel.SelectedLocations
        };
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }
        
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-many-locations/add-new-location", Name = RouteNames.AddNewLocationJourney_Post)]
    public IActionResult AddALocation(AddMoreThanOneLocationEditModel editModel, [FromQuery] bool wizard)
    {
        TempData.Remove(TempDataKeys.Postcode);
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