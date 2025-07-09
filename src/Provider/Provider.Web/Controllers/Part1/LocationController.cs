using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class LocationController(IWebHostEnvironment hostingEnvironment) : EmployerControllerBase(hostingEnvironment)
{
    private static readonly Dictionary<string, Tuple<string, string>> ValidationMappings = new()
    {
        { "EmployerLocations", Tuple.Create<string, string>("SelectedLocation", null) },
        { VacancyValidationErrorCodes.AddressCountryNotInEngland, Tuple.Create("SelectedLocation", "Location must be in England. Your apprenticeship must be in England to advertise it on this service") },
    };
    
    [HttpGet("add-one-location", Name = RouteNames.AddOneLocation_Get)]
    public async Task<IActionResult> AddOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        VacancyRouteModel vacancyRouteModel,
        [FromQuery] bool wizard)
    {
        ModelState.ThrowIfBindingErrors();
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
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }
        
        if (TempData[TempDataKeys.AddedLocation] is string newlyAddedLocation)
        {
            viewModel.SelectedLocation = newlyAddedLocation;
            viewModel.BannerAddress = newlyAddedLocation;
        }
        return View(viewModel);
    }
    
    [HttpPost("add-one-location", Name = RouteNames.AddOneLocation_Post)]
    public async Task<IActionResult> AddOneLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
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
            return utility.IsTaskListCompleted(vacancy)
                ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { model.VacancyId, model.Ukprn, wizard })
                : RedirectToRoute(RouteNames.ProviderTaskListGet, new { model.VacancyId, model.Ukprn, wizard });
        }

        ModelState.AddValidationErrorsWithMappings(result.ValidationResult, ValidationMappings);
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
            if (vacancy.Status == VacancyStatus.Referred)
            {
                viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
            }
        return View(viewModel);
    }
}