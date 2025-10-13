﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.RecruitNationally;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
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
    
    [HttpGet("location-information", Name = RouteNames.RecruitNationally_Get)]
    public async Task<IActionResult> RecruitNationally([FromServices] IUtility utility, [FromServices] IReviewSummaryService reviewSummaryService, VacancyRouteModel model, [FromQuery] bool wizard = true)
    {
        ModelState.ThrowIfBindingErrors();
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            AdditionalInformation = vacancy.EmployerLocationInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }

        return View(viewModel);
    }
    
    [HttpPost("location-information", Name = RouteNames.RecruitNationally_Post)]
    public async Task<IActionResult> RecruitNationally(
        [FromServices] IVacancyLocationService vacancyLocationService,
        [FromServices] IUtility utility,
        [FromServices] IReviewSummaryService reviewSummaryService,
        RecruitNationallyEditModel model,
        [FromQuery] bool wizard = true)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model);
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
        
        ModelState.AddValidationErrorsWithFieldMappings(result.ValidationResult, ValidationFieldMappings);
        var viewModel = new RecruitNationallyViewModel
        {
            ApprenticeshipTitle = vacancy.Title,
            EmployerAccountId = model.EmployerAccountId,
            AdditionalInformation = model.AdditionalInformation,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyId = model.VacancyId,
        };
        viewModel.PageInfo.SetWizard(wizard);
        if (vacancy.Status == VacancyStatus.Referred)
        {
            viewModel.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetWhereIsApprenticeshipAvailableFieldIndicators());
        }
        
        return View(viewModel);
    }
}