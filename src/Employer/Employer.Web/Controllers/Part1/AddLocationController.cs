using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class AddLocationController(IUtility utility) : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("add-location", Name = RouteNames.AddLocation_Get)]
    public async Task<IActionResult> AddLocation(AddLocationModel model)
    {
        string postcode = TempData[TempDataKeys.Postcode] as string;
        var viewModel = await GetAddLocationViewModel(utility, model, postcode, model.Origin, model.Wizard);
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("add-location", Name = RouteNames.AddLocation_Post)]
    public async Task<IActionResult> AddLocation(
        [FromServices] IValidator<AddLocationEditModel> validator,
        [FromServices] IGetAddressesClient getAddressesClient,
        AddLocationEditModel model, 
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (validationResult.IsValid)
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
            ModelState.AddModelError(nameof(AddLocationEditModel.Postcode), validationResult.ToString());
        }
        
        var viewModel = await GetAddLocationViewModel(utility, model, model.Postcode, model.Origin, model.Wizard);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("select-location", Name = RouteNames.SelectAnAddress_Get)]
    public async Task<IActionResult> SelectLocation([FromServices] IGetAddressesClient getAddressesClient, SelectLocationModel model)
    {
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        if (string.IsNullOrEmpty(postcode))
        {
            return RedirectToRoute(RouteNames.AddLocation_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard, model.Origin });
        }
        var viewModel = await GetSelectLocationViewModel(getAddressesClient, utility, model, postcode, RouteNames.SelectAnAddress_Get);
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("select-location", Name = RouteNames.SelectAnAddress_Post)]
    public async Task<IActionResult> SelectLocation([FromServices] IRecruitVacancyClient recruitVacancyClient, [FromServices] IGetAddressesClient getAddressesClient, SelectLocationEditModel model)
    {
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        
        if (string.IsNullOrEmpty(postcode))
        {
            return RedirectToRoute(RouteNames.AddLocation_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard, model.Origin });
        }
        
        var newAddress = await SaveSelectedLocation(recruitVacancyClient, getAddressesClient, model, postcode, model.SelectedLocation);
        if (newAddress is null)
        {
            var viewModel = await GetSelectLocationViewModel(getAddressesClient, utility, model, postcode, RouteNames.SelectAnAddress_Post);
            return View(viewModel);
        }
        
        TempData[TempDataKeys.AddedLocation] = newAddress.ToAddressString();
        TempData.Remove(TempDataKeys.Postcode);
        return model.Origin switch
        {
            MultipleLocationsJourneyOrigin.One => throw new NotImplementedException(),
            MultipleLocationsJourneyOrigin.Many => RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard }),
            _ => throw new NotImplementedException()
        };
    }

    private async Task<Address> SaveSelectedLocation(
        IRecruitVacancyClient recruitVacancyClient,
        IGetAddressesClient getAddressesClient,
        SelectLocationEditModel model,
        string postcode,
        string selectedLocation)
    {
        if (!ModelState.IsValid)
        {
            return null;
        }

        var response = await getAddressesClient.GetAddresses(postcode);
        var addressItem = response?.Addresses.FirstOrDefault(x => x.ToShortAddress() == selectedLocation);
        if (addressItem is null)
        {
            return null;
        }
        var addressToAdd = addressItem.ToDomain(); 
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.SelectAnAddress_Post);
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        employerProfile.OtherLocations.Add(addressToAdd);
        await recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, User.ToVacancyUser());

        return addressToAdd;
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
    
    private static async Task<SelectLocationViewModel> GetSelectLocationViewModel(IGetAddressesClient getAddressesClient, IUtility utility, SelectLocationModel model, string postcode, string routeName)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, routeName);
        var response = await getAddressesClient.GetAddresses(postcode);
        var viewModel = new SelectLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            Locations = response?.Addresses?.Select(x => x.ToShortAddress()).ToList() ?? [],
            Origin = model.Origin,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            Postcode = postcode,
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(model.Wizard);
        return viewModel;
    }
}