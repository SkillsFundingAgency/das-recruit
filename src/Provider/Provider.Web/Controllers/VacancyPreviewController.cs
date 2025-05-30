﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyPreviewController : Controller
    {
        private readonly VacancyPreviewOrchestrator _orchestrator;


        public VacancyPreviewController(VacancyPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("advert-preview", Name = RouteNames.Preview_Submit_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);

            if (!ModelState.IsValid)
            {
                viewModel.SoftValidationErrors = null;
                return View("AdvertPreview", viewModel);
            }

            var response = await _orchestrator.SubmitVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
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

            viewModel.SoftValidationErrors = null;
            return View(ViewNames.VacancyPreview, viewModel);
        }
        
        [HttpGet("advert-preview", Name = RouteNames.Vacancy_Advert_Preview_Get)]
        public async Task<IActionResult> AdvertPreview(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);

            if (TempData.ContainsKey(TempDataKeys.VacancyPreviewInfoMessage))
                viewModel.InfoMessage = TempData[TempDataKeys.VacancyPreviewInfoMessage].ToString();

            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.CanHideValidationSummary = true;
            return View(ViewNames.AdvertPreview , viewModel);
        }

        private void AddSoftValidationErrorsToModelState(VacancyPreviewViewModel viewModel)
        {
            if (!viewModel.SoftValidationErrors.HasErrors)
                return;

            foreach (var error in viewModel.SoftValidationErrors.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}