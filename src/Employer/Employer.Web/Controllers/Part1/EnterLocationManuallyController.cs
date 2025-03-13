using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class EnterLocationManuallyController(IUtility utility) : Controller
{
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("enter-location", Name = RouteNames.EnterAddressManually_Get)]
    public async Task<IActionResult> EnterLocationManually(AddLocationJourneyModel model)
    {
        string returnRoute = TempData.Peek(TempDataKeys.AddLocationReturnPath) as string;
        string postcode = TempData.Peek(TempDataKeys.Postcode) as string;
        var viewModel = await GetEnterLocationManuallyViewModel(utility, model, RouteNames.EnterAddressManually_Get, new Address { Postcode = postcode }, returnRoute);
        return View(viewModel);
    }

    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("enter-location", Name = RouteNames.EnterAddressManually_Post)]
    public async Task<IActionResult> EnterLocationManually(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IValidator<EnterLocationManuallyEditModel> validator,
        EnterLocationManuallyEditModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            string returnRoute = TempData.Peek(TempDataKeys.AddLocationReturnPath) as string;
            var viewModel = await GetEnterLocationManuallyViewModel(utility, model, RouteNames.EnterAddressManually_Get, model.ToDomain(), returnRoute);
            return View(viewModel);
        }
        
        var newAddress = model.ToDomain();
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.SelectAnAddress_Post);
        await vacancyLocationService.SaveEmployerAddress(User.ToVacancyUser(), vacancy, newAddress);
        
        TempData[TempDataKeys.AddedLocation] = newAddress.ToAddressString();
        TempData.Remove(TempDataKeys.Postcode);
        return model.Origin == MultipleLocationsJourneyOrigin.One
            ? RedirectToRoute(RouteNames.AddOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard })
            : RedirectToRoute(RouteNames.AddMoreThanOneLocation_Get, new { model.VacancyId, model.EmployerAccountId, model.Wizard });
    }

    private static async Task<EnterLocationManuallyViewModel> GetEnterLocationManuallyViewModel(IUtility utility, AddLocationJourneyModel model, string routeName, Address address, string returnRoute)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, routeName);
        var viewModel = new EnterLocationManuallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
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