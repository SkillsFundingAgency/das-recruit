using System.Linq;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
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
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active";
        private const string InvalidSearchTerm = "Please enter a training provider name or UKPRN";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator, IRecruitVacancyClient vacancyClient)
        {
            _orchestrator = orchestrator;
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
           vm.ReferredFromMAHome_FromSavedFavourites = ShowReferredFromMABackLink();
            return View(vm);
        }

        private bool ShowReferredFromMABackLink()
        {
            var referredFromMAHome_FromSavedFavourites = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMAHome_FromSavedFavourites));
            var referredFromMAHome_UKPRN = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_UKPRN));
            var referredFromMAHome_ProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_ProgrammeId));
            return referredFromMAHome_FromSavedFavourites && string.IsNullOrWhiteSpace(referredFromMAHome_UKPRN) && !string.IsNullOrWhiteSpace(referredFromMAHome_ProgrammeId);
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
                        ? RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, new { ukprn = response.Data.FoundTrainingProviderUkprn.Value, wizard })
                        : GetRedirectToNextPage(wizard);
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
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);
            
            var vm = await _orchestrator.GetConfirmViewModelAsync(vrm, provider.Ukprn);
            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> ConfirmTrainingProvider(ConfirmTrainingProviderEditModel m, [FromQuery] bool wizard)
        {
            if (ModelState.IsValid)
            {
                var response = await _orchestrator.PostConfirmEditModelAsync(m, User.ToVacancyUser());

                if(response.Success)
                    return GetRedirectToNextPage(wizard);
                
                AddTrainingProviderErrorsToModelState(TrainingProviderSelectionType.Ukprn, m.Ukprn, response, ModelState);
            }

            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
            vm.PageInfo.SetWizard(wizard);
            return View(ViewNames.SelectTrainingProvider, vm);
        }
        
        private IActionResult GetRedirectToNextPage(bool wizard)
        {
            return wizard
                ? RedirectToRoute(RouteNames.NumberOfPositions_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
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
                        AddProviderNotFoundErrorToModelState(selectionType, ukprn);
                        continue;
                    }
                    
                    if (error.ErrorCode == ErrorCodes.TrainingProviderMustNotBeBlocked)
                    {
                        AddProviderBlockedErrorToModelState(selectionType, error);
                        continue;
                    }
                    
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }

        private void AddProviderNotFoundErrorToModelState(TrainingProviderSelectionType selectionType, string searchedUkprn)
        {
            if (selectionType == TrainingProviderSelectionType.Ukprn)
            {
                ModelState.AddModelError(nameof(SelectTrainingProviderEditModel.Ukprn), string.Format(InvalidUkprnMessageFormat, searchedUkprn));
            }
            else
            {
                ModelState.AddModelError(nameof(SelectTrainingProviderEditModel.TrainingProviderSearch), InvalidSearchTerm);
            }
        }

        private void AddProviderBlockedErrorToModelState(TrainingProviderSelectionType selectionType, EntityValidationError error)
        {
            var errorKey = selectionType == TrainingProviderSelectionType.Ukprn ? nameof(SelectTrainingProviderEditModel.Ukprn) : nameof(SelectTrainingProviderEditModel.TrainingProviderSearch);

            ModelState.AddModelError(errorKey, error.ErrorMessage);
        }
    }
}