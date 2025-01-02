using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            AdditionalInformation = vacancy.EmployerLocationInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);

        return View(viewModel);
    }
    
    [FeatureGate(FeatureNames.MultipleLocations)]
    [HttpPost("location-information", Name = RouteNames.RecruitNationally_Post)]
    public async Task<IActionResult> RecruitNationally(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        RecruitNationallyEditModel model,
        [FromQuery] bool wizard = true)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.AddMoreThanOneLocation_Post);
        var result = await vacancyLocationService.UpdateDraftVacancyLocations(
            vacancy,
            User.ToVacancyUser(),
            AvailableWhere.AcrossEngland,
            null,
            model.AdditionalInformation);

        if (result.ValidationResult is null)
        {
            return wizard
                ? RedirectToRoute(RouteNames.EmployerTaskListGet, new {model.VacancyId, model.EmployerAccountId, wizard}) 
                : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {model.VacancyId, model.EmployerAccountId});    
        }
        
        ModelState.AddValidationErrors(result.ValidationResult, ValidationFieldMappings);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            AdditionalInformation = model.AdditionalInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        
        return View(viewModel);
    }
}