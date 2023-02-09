using System;
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
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyPreviewController : Controller
    {
        private readonly VacancyPreviewOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;

        public VacancyPreviewController(VacancyPreviewOrchestrator orchestrator, ServiceParameters serviceParameters)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
        }

        [HttpGet("preview", Name = RouteNames.Vacancy_Preview_Get)]
        public async Task<IActionResult> VacancyPreview(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);

            if (TempData.ContainsKey(TempDataKeys.VacancyPreviewInfoMessage))
                viewModel.InfoMessage = TempData[TempDataKeys.VacancyPreviewInfoMessage].ToString();

            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.SetSectionStates(viewModel, ModelState);

            viewModel.CanHideValidationSummary = true;

            return View(viewModel);
        }

        [HttpPost("preview", Name = RouteNames.Preview_Submit_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
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

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);

            viewModel.SoftValidationErrors = null;
            viewModel.SetSectionStates(viewModel, ModelState);

            return View(ViewNames.VacancyPreview, viewModel);
        }
        
        [HttpGet("advert-preview", Name = RouteNames.Vacancy_Advert_Preview_Get)]
        public async Task<IActionResult> AdvertPreview(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);

            if (TempData.ContainsKey(TempDataKeys.VacancyPreviewInfoMessage))
                viewModel.InfoMessage = TempData[TempDataKeys.VacancyPreviewInfoMessage].ToString();

            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.SetSectionStates(viewModel, ModelState);

            viewModel.CanHideValidationSummary = true;

            var isApprenticeship = _serviceParameters.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship;
            return View(isApprenticeship ? ViewNames.AdvertPreview : ViewNames.AdvertPreviewTraineeship, viewModel);
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