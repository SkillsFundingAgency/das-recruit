using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class SelectLocationController(IUtility utility) : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("select-location", Name = RouteNames.SelectAnAddress_Get)]
    public async Task<IActionResult> SelectLocation([FromServices] IGetAddressesClient getAddressesClient, AddLocationJourneyModel model)
    {
        TempData[TempDataKeys.AddLocationReturnPath] = RouteNames.SelectAnAddress_Get;
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        if (string.IsNullOrEmpty(postcode))
        {
            return RedirectToRoute(RouteNames.AddLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard, model.Origin });
        }
        var viewModel = await GetSelectLocationViewModel(getAddressesClient, utility, model, postcode, RouteNames.SelectAnAddress_Get);
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("select-location", Name = RouteNames.SelectAnAddress_Post)]
    public async Task<IActionResult> SelectLocation(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IGetAddressesClient getAddressesClient, 
        SelectLocationEditModel model)
    {
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        if (string.IsNullOrEmpty(postcode))
        {
            return RedirectToRoute(RouteNames.AddLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard, model.Origin });
        }
        
        var addressToAdd = ModelState.IsValid ? await LookupAddress(getAddressesClient, postcode, model.SelectedLocation) : null;
        if (addressToAdd is null)
        {
            var viewModel = await GetSelectLocationViewModel(getAddressesClient, utility, model, postcode, RouteNames.SelectAnAddress_Post);
            return View(viewModel);
        }
        
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.SelectAnAddress_Post);
        await vacancyLocationService.SaveEmployerAddress(User.ToVacancyUser(), vacancy, model.Ukprn, addressToAdd);
        
        TempData[TempDataKeys.AddedLocation] = addressToAdd.ToAddressString();
        TempData.Remove(TempDataKeys.Postcode);
        return model.Origin == MultipleLocationsJourneyOrigin.One
            ? RedirectToRoute(RouteNames.AddOneLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard })
            : RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard });
    }

    private static async Task<Address> LookupAddress(IGetAddressesClient getAddressesClient, string postcode, string selectedLocation)
    {
        var response = await getAddressesClient.GetAddresses(postcode);
        var addressItem = response?.Addresses.FirstOrDefault(x => x.ToShortAddress() == selectedLocation);
        return addressItem?.ToDomain();
    }

    private static async Task<SelectLocationViewModel> GetSelectLocationViewModel(IGetAddressesClient getAddressesClient, IUtility utility, AddLocationJourneyModel model, string postcode, string routeName)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, routeName);
        var response = await getAddressesClient.GetAddresses(postcode);
        var viewModel = new SelectLocationViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            Ukprn = model.Ukprn,
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