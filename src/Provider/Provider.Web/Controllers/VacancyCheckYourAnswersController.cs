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
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class VacancyCheckYourAnswersController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;
        private readonly bool _isFaaV2Enabled;

        public VacancyCheckYourAnswersController (VacancyTaskListOrchestrator orchestrator, ServiceParameters serviceParameters, IConfiguration configuration, IFeature feature)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
            _isFaaV2Enabled = feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements);
        }
        
        [HttpGet("{vacancyId:guid}/check-your-answers", Name = RouteNames.ProviderCheckYourAnswersGet)]
        public async Task<IActionResult> CheckYourAnswers(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.CanHideValidationSummary = true;
            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);
            
            if (TempData.ContainsKey(TempDataKeys.VacancyPreviewInfoMessage))
                viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyPreviewInfoMessage].ToString();
            
            return View(viewModel);
        }
        
        [HttpPost("{vacancyId:guid}/check-your-answers", Name = RouteNames.ProviderCheckYourAnswersPost)]
        public async Task<IActionResult> CheckYourAnswers(SubmitEditModel m)
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

            var viewModel = await _orchestrator.GetVacancyTaskListModel(m);

            viewModel.SoftValidationErrors = null;
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);
            viewModel.ValidationErrors = new ValidationSummaryViewModel
                {ModelState = ModelState, OrderedFieldNames = viewModel.OrderedFieldNames};
            
            return View(viewModel);
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