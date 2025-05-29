﻿using System.Linq;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private readonly IFeature _feature;
        private const string InvalidSearchTerm = "Enter the name or UKPRN of a training provider who is registered to deliver apprenticeship training";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("select-training-provider", Name = RouteNames.TrainingProvider_Select_Get)]
        public async Task<IActionResult> SelectTrainingProvider(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] long? ukprn = null)
        {
           var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(vrm, ukprn);
           vm.PageInfo.SetWizard(wizard);

           if (string.IsNullOrWhiteSpace(clear) == false)
           {
               vm.Ukprn = string.Empty;
               vm.TrainingProviderSearch = string.Empty;
               vm.IsTrainingProviderSelected = true;
           }
           
           vm.BackLinkRoute = GetBackLinkRoute(vrm.VacancyId);
           return View(vm);
        }

        private string GetBackLinkRoute(Guid vacancyId)
        {
            var referredUkprn = Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn + vacancyId));
            var referredProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId + vacancyId));
            if (!string.IsNullOrWhiteSpace(referredUkprn) || !string.IsNullOrWhiteSpace(referredProgrammeId))
                return RouteNames.Title_Get;
            return RouteNames.Training_Get;
        }

        [HttpPost("select-training-provider", Name = RouteNames.TrainingProvider_Select_Post)]
        public async Task<IActionResult> SelectTrainingProvider(SelectTrainingProviderEditModel m, [FromQuery] bool wizard)
        {
            if (ModelState.IsValid)
            {
                var response = await _orchestrator.PostSelectTrainingProviderAsync(m, User.ToVacancyUser());

                if (response.Success)
                {
                    return response.Data.FoundTrainingProviderUkprn.HasValue
                        ? RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, new { ukprn = response.Data.FoundTrainingProviderUkprn.Value, wizard, m.VacancyId, m.EmployerAccountId })
                        : GetRedirectToNextPage(wizard, m);
                }

                AddTrainingProviderErrorsToModelState(m.SelectionType, m.Ukprn, response, ModelState);
            }

            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProvider(VacancyRouteModel vrm, [FromQuery] string ukprn, [FromQuery] string wizard)
        {
            var provider = await _orchestrator.GetProviderAsync(ukprn);
            
            if(provider == null)
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get, new {vrm.VacancyId, vrm.EmployerAccountId});
            
            var vm = await _orchestrator.GetConfirmViewModelAsync(vrm, provider.Ukprn);
            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> ConfirmTrainingProvider(
            [FromServices] IRecruitVacancyClient vacancyClient,
            [FromServices] ITaskListValidator taskListValidator,
            ConfirmTrainingProviderEditModel m,
            [FromQuery] bool wizard)
        {
            if (ModelState.IsValid)
            {
                var response = await _orchestrator.PostConfirmEditModelAsync(m, User.ToVacancyUser());
                if (response.Success)
                {
                    var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId);
                    bool isTaskListCompleted = await taskListValidator.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.All);
                    return GetRedirectToNextPage(!isTaskListCompleted, m);
                }
                
                AddTrainingProviderErrorsToModelState(TrainingProviderSelectionType.Ukprn, m.Ukprn, response, ModelState);
            }

            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
            vm.PageInfo.SetWizard(wizard);
            return View(ViewNames.SelectTrainingProvider, vm);
        }
        
        private IActionResult GetRedirectToNextPage(bool wizard, VacancyRouteModel vrm)
        {
            if (wizard)
            {
                return RedirectToRoute(RouteNames.ShortDescription_Get, new { wizard, vrm.VacancyId, vrm.EmployerAccountId});
            }

            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { vrm.VacancyId, vrm.EmployerAccountId});
        }

        public void AddTrainingProviderErrorsToModelState(TrainingProviderSelectionType selectionType, string ukprn, OrchestratorResponse response, ModelStateDictionary modelState)
        {
            string[] providerNotFoundErrorCodes = { ErrorCodes.TrainingProviderUkprnNotEmpty, ErrorCodes.TrainingProviderMustExist };

            if (!response.Success && response.Errors?.Errors != null)
            {
                foreach (var error in response.Errors.Errors)
                {
                    if (providerNotFoundErrorCodes.Contains(error.ErrorCode))
                    {
                        AddProviderNotFoundErrorToModelState();
                        continue;
                    }
                    
                    if (error.ErrorCode is ErrorCodes.TrainingProviderMustNotBeBlocked)
                    {
                        AddProviderBlockedErrorToModelState(selectionType, error);
                        continue;
                    }
                    
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }

        private void AddProviderNotFoundErrorToModelState()
        {
            ModelState.AddModelError(nameof(SelectTrainingProviderEditModel.TrainingProviderSearch), InvalidSearchTerm);
        }

        private void AddProviderBlockedErrorToModelState(TrainingProviderSelectionType selectionType, EntityValidationError error)
        {
            var errorKey = selectionType == TrainingProviderSelectionType.Ukprn ? nameof(SelectTrainingProviderEditModel.Ukprn) : nameof(SelectTrainingProviderEditModel.TrainingProviderSearch);

            ModelState.AddModelError(errorKey, error.ErrorMessage);
        }
    }
}