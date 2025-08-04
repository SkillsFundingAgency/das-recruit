﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class AddLocationController(IUtility utility) : Controller
{
    [HttpGet("add-location", Name = RouteNames.AddLocation_Get)]
    public async Task<IActionResult> AddLocation(AddLocationJourneyModel model)
    {
        ModelState.ThrowIfBindingErrors();
        TempData[TempDataKeys.AddLocationReturnPath] = RouteNames.AddLocation_Get;
        string postcode = TempData[TempDataKeys.Postcode] as string;
        var viewModel = await GetAddLocationViewModel(utility, model, postcode, model.Origin, model.Wizard);
        return View(viewModel);
    }
    
    [HttpPost("add-location", Name = RouteNames.AddLocation_Post)]
    public async Task<IActionResult> AddLocation(
        [FromServices] ILocationsService locationsService,
        [FromServices] IGetAddressesClient getAddressesClient,
        AddLocationEditModel model)
    {
        if (ModelState.IsValid)
        {
            bool? isPostcodeEnglish = await locationsService.IsPostcodeInEnglandAsync(model.Postcode);
            if (isPostcodeEnglish is not false)
            {
                var response = await getAddressesClient.GetAddresses(model.Postcode);
                if (response is not null)
                {
                    TempData[TempDataKeys.Postcode] = model.Postcode.ToUpperInvariant();
                    return RedirectToRoute(RouteNames.SelectAnAddress_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard, model.Origin });
                }
            
                ModelState.AddModelError(nameof(AddLocationEditModel.Postcode), AddLocationEditModelValidator.InvalidPostcodeErrorMessage);
            }
            else
            {
                ModelState.AddModelError(nameof(AddLocationEditModel.Postcode), AddLocationEditModelValidator.MustBeEnglishPostcode);    
            }
        }
        
        var viewModel = await GetAddLocationViewModel(utility, model, model.Postcode, model.Origin, model.Wizard);
        return View(viewModel);
    }

    private static async Task<AddLocationViewModel> GetAddLocationViewModel(IUtility utility, VacancyRouteModel vacancyRouteModel, string postcode, MultipleLocationsJourneyOrigin origin, bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Location_Get);
        var viewModel = new AddLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = vacancyRouteModel.EmployerAccountId,
            Origin = origin,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            Postcode = postcode,
            VacancyId = vacancyRouteModel.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        return viewModel;
    }
}