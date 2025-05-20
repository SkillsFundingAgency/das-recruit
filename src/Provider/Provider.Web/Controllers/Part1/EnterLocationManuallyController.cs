using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Provider.Web.ViewModels.Validations;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class EnterLocationManuallyController(IUtility utility) : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("enter-location", Name = RouteNames.EnterAddressManually_Get)]
    public async Task<IActionResult> EnterLocationManually(AddLocationJourneyModel model)
    {
        ModelState.ThrowIfBindingErrors();
        string returnRoute = TempData.Peek(TempDataKeys.AddLocationReturnPath) as string;
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        var viewModel = await GetEnterLocationManuallyViewModel(utility, model, RouteNames.EnterAddressManually_Get, new Address { Postcode = postcode }, returnRoute);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("enter-location", Name = RouteNames.EnterAddressManually_Post)]
    public async Task<IActionResult> EnterLocationManually(
        [FromServices] ILocationsService locationsService,
        [FromServices] IVacancyLocationService vacancyLocationService,
        EnterLocationManuallyEditModel model)
    {
        if (!ModelState.IsValid)
        {
            string returnRoute = TempData.Peek(TempDataKeys.AddLocationReturnPath) as string;
            var viewModel = await GetEnterLocationManuallyViewModel(utility, model, RouteNames.EnterAddressManually_Get, model.ToDomain(), returnRoute);
            return View(viewModel);
        }
        
        bool? isPostcodeEnglish = await locationsService.IsPostcodeInEnglandAsync(model.Postcode);
        if (isPostcodeEnglish is false)
        {
            ModelState.AddModelError(nameof(AddLocationEditModel.Postcode), AddLocationEditModelValidator.MustBeEnglishPostcode);
            string returnRoute = TempData.Peek(TempDataKeys.AddLocationReturnPath) as string;
            var viewModel = await GetEnterLocationManuallyViewModel(utility, model, RouteNames.EnterAddressManually_Get, model.ToDomain(), returnRoute);
            return View(viewModel);
        }
        
        var newAddress = model.ToDomain();
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.SelectAnAddress_Post);
        await vacancyLocationService.SaveEmployerAddress(User.ToVacancyUser(), vacancy, model.Ukprn, newAddress);
        
        TempData[TempDataKeys.AddedLocation] = newAddress.ToAddressString();
        TempData.Remove(TempDataKeys.Postcode);
        return model.Origin == MultipleLocationsJourneyOrigin.One
            ? RedirectToRoute(RouteNames.AddOneLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard })
            : RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.Ukprn, model.Wizard });
    }

    private static async Task<EnterLocationManuallyViewModel> GetEnterLocationManuallyViewModel(IUtility utility, AddLocationJourneyModel model, string routeName, Address address, string returnRoute)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, routeName);
        var viewModel = new EnterLocationManuallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            Ukprn = model.Ukprn,
            Origin = model.Origin,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            ReturnRoute = returnRoute,
            VacancyId = model.VacancyId,
            Wizard = model.Wizard,
            
            // Address
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.AddressLine3,
            Postcode = address.Postcode,
        };
        viewModel.PageInfo.SetWizard(model.Wizard);
        return viewModel;
    }
}