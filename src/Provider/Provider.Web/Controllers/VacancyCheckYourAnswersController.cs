using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class VacancyCheckYourAnswersController(VacancyCheckYourAnswersOrchestrator orchestrator) : Controller
{
    private static readonly Dictionary<string, Tuple<string, string>> ValidationMappings = new()
    {
        { "EmployerLocations", Tuple.Create<string, string>("EmployerAddress", null) },
        { VacancyValidationErrorCodes.AddressCountryNotInEngland, Tuple.Create("EmployerAddress", "Location must be in England. Your apprenticeship must be in England to advertise it on this service") },
        { $"Multiple-{VacancyValidationErrorCodes.AddressCountryNotInEngland}", Tuple.Create("EmployerAddress", "All locations must be in England. Your apprenticeship must be in England to advertise it on this service") },
    };

    [HttpGet("{vacancyId:guid}/check-your-answers", Name = RouteNames.ProviderCheckYourAnswersGet)]
    public async Task<IActionResult> CheckYourAnswers(VacancyRouteModel vrm)
    {
        var viewModel = await orchestrator.GetVacancyTaskListModel(vrm);
        viewModel.CanHideValidationSummary = !viewModel.SoftValidationErrors.HasErrors;
        ModelState.AddValidationErrorsWithMappings(viewModel.SoftValidationErrors, ValidationMappings);
        viewModel.ValidationErrors = new ValidationSummaryViewModel
        {
            ModelState = ModelState,
            OrderedFieldNames = viewModel.OrderedFieldNames
        };

        if (TempData.ContainsKey(TempDataKeys.VacancyPreviewInfoMessage))
        {
            viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyPreviewInfoMessage].ToString();
        }

        if (TempData[TempDataKeys.LegalEntityChanged] is bool legalEntityChanged)
        {
            viewModel.ShowReviewVacancyAsEmployerHasChangedBanner = legalEntityChanged;
        }
            
        return View(viewModel);
    }
        
    [HttpPost("{vacancyId:guid}/check-your-answers", Name = RouteNames.ProviderCheckYourAnswersPost)]
    public async Task<IActionResult> CheckYourAnswers(SubmitEditModel m)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await orchestrator.GetVacancyTaskListModel(m);
            viewModel.SoftValidationErrors = null;
            viewModel.HasUserConfirmation = m.HasUserConfirmation;
            viewModel.ValidationErrors = new ValidationSummaryViewModel
                { ModelState = ModelState, OrderedFieldNames = viewModel.OrderedFieldNames };
            return View(viewModel);
        }

        var response = await orchestrator.SubmitVacancyAsync(m, User.ToVacancyUser());
            
        if (!response.Success)
        {
            ModelState.AddValidationErrorsWithMappings(response.Errors, ValidationMappings);
        }

        if (ModelState.IsValid)
        {
            if (response.Data.IsSubmitted)
                return RedirectToRoute(RouteNames.Submitted_Index_Get, m.RouteDictionary);

            if (response.Data.IsSentForReview)
                return RedirectToRoute(RouteNames.Reviewed_Index_Get, m.RouteDictionary);

            if (response.Data.HasProviderAgreement == false)
                return RedirectToRoute(RouteNames.ProviderAgreement_HardStop_Get, m.RouteDictionary);

            if (response.Data.HasLegalEntityAgreement == false)
                return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get, m.RouteDictionary);

            throw new Exception("Unknown submit state");
        }

        if(response.Errors.Errors.Any(e => e.ErrorCode == ErrorCodes.TrainingProviderMustHaveEmployerPermission))
            throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, m.Ukprn));

        var vm = await orchestrator.GetVacancyTaskListModel(m);

        vm.SoftValidationErrors = null;
        vm.ValidationErrors = new ValidationSummaryViewModel
            {ModelState = ModelState, OrderedFieldNames = vm.OrderedFieldNames};
            
        return View(vm);
    }
}