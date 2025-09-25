using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
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
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel);
            var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy);
            var selectedLocation = vacancy.EmployerLocations is { Count: 1 } ? vacancy.EmployerLocations[0] : null;

            var viewModel = new AddOneLocationViewModel
            {
                ApprenticeshipTitle = vacancy.Title,
                AvailableLocations = allLocations ?? [],
                VacancyId = vacancyRouteModel.VacancyId,
                EmployerAccountId = vacancyRouteModel.EmployerAccountId,
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
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model);
            var allLocations = await vacancyLocationService.GetVacancyLocations(vacancy);
            var location = allLocations.FirstOrDefault(x => x.ToAddressString() == model.SelectedLocation);
            var result = await vacancyLocationService.UpdateDraftVacancyLocations(
                vacancy,
                User.ToVacancyUser(),
                AvailableWhere.OneLocation,
                location is null ? null : [location]);

            if (result.ValidationResult is null)
            {
                return wizard
                    ? RedirectToRoute(RouteNames.EmployerTaskListGet, new { model.VacancyId, model.EmployerAccountId, wizard })
                    : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { model.VacancyId, model.EmployerAccountId });
            }

            ModelState.AddValidationErrorsWithMappings(result.ValidationResult, ValidationMappings);
            var viewModel = new AddOneLocationViewModel
            {
                ApprenticeshipTitle = vacancy.Title,
                AvailableLocations = allLocations,
                VacancyId = model.VacancyId,
                EmployerAccountId = model.EmployerAccountId,
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
}