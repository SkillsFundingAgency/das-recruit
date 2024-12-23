using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class RecruitNationallyController: Controller
{
    private static readonly Dictionary<string, string> ValidationFieldMappings = new()
    {
        { "EmployerLocationInformation", "AdditionalInformation" }
    };
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpGet("location-information", Name = RouteNames.RecruitNationally_Get)]
    public async Task<IActionResult> RecruitNationally([FromServices] IUtility utility, VacancyRouteModel model, [FromQuery] bool wizard = true)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.RecruitNationally_Get);
        var viewModel = await GetViewModel(utility, model, RouteNames.RecruitNationally_Get, vacancy.EmployerLocationInformation, wizard);
        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-information", Name = RouteNames.RecruitNationally_Post)]
    public async Task<IActionResult> RecruitNationally(
        [FromServices] IRecruitVacancyClient recruitVacancyClient,
        [FromServices] IUtility utility,
        RecruitNationallyEditModel model,
        [FromQuery] bool wizard = true)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddMoreThanOneLocation_Post);
        vacancy.EmployerLocationOption = AvailableWhere.AcrossEngland;
        vacancy.EmployerLocationInformation = model.AdditionalInformation;
        vacancy.EmployerLocation = null;
        vacancy.EmployerLocations = null;

        var validationResult = recruitVacancyClient.Validate(vacancy, VacancyRuleSet.EmployerAddress);
        if (validationResult.HasErrors)
        {
            ModelState.AddValidationErrors(validationResult, ValidationFieldMappings);
            var viewModel = await GetViewModel(utility, model, RouteNames.RecruitNationally_Post, model.AdditionalInformation, wizard);
            return View(viewModel);
        }

        await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, User.ToVacancyUser());
        return wizard
            ? RedirectToRoute(RouteNames.EmployerTaskListGet, new {model.VacancyId, model.EmployerAccountId, wizard}) 
            : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {model.VacancyId, model.EmployerAccountId});
    }

    private static async Task<RecruitNationallyViewModel> GetViewModel(IUtility utility, VacancyRouteModel model, string routeName, string locationAdditionalInfo, bool wizard)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, routeName);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            AdditionalInformation = locationAdditionalInfo,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return viewModel;
    }
}