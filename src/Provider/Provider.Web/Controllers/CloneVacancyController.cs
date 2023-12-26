using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class CloneVacancyController : Controller
    {
        private readonly CloneVacancyOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public CloneVacancyController(CloneVacancyOrchestrator orchestrator, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("clone", Name = RouteNames.CloneVacancy_Get)]
        public async Task<IActionResult> Clone(VacancyRouteModel vrm)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            var vacancy = await _orchestrator.GetCloneableAuthorisedVacancyAsync(vrm);

            return _orchestrator.IsNewDatesRequired(vacancy) 
                ? RedirectToRoute(RouteNames.CloneVacancyWithNewDates_Get, new {vrm.Ukprn, vrm.VacancyId}) 
                : RedirectToRoute(RouteNames.CloneVacancyDatesQuestion_Get, new {vrm.Ukprn, vrm.VacancyId});
        }

        [HttpGet("clone-dates-question", Name = RouteNames.CloneVacancyDatesQuestion_Get)]
        public async Task<IActionResult> CloneVacancyDatesQuestion(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCloneVacancyDatesQuestionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("clone-dates-question", Name = RouteNames.CloneVacancyDatesQuestion_Post)]
        public async Task<IActionResult> CloneVacancyDatesQuestion(CloneVacancyDatesQuestionEditModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetCloneVacancyDatesQuestionViewModelAsync(model);
                return View(vm);
            }

            if (model.HasConfirmedClone == true)
            {
                var newVacancyId = await _orchestrator.PostCloneVacancyWithSameDates(model, User.ToVacancyUser());
                TempData.TryAdd(TempDataKeys.VacancyPreviewInfoMessage, InfoMessages.VacancyCloned);
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { VacancyId = newVacancyId, model.Ukprn });
            }
            else
            {
                return RedirectToRoute(RouteNames.CloneVacancyWithNewDates_Get, new { model.VacancyId, model.Ukprn });
            }
        }

        [HttpGet("clone-with-dates", Name = RouteNames.CloneVacancyWithNewDates_Get)]
        public async Task<IActionResult> CloneVacancyWithNewDates(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCloneVacancyWithNewDatesViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("clone-with-dates", Name = RouteNames.CloneVacancyWithNewDates_Post)]
        public async Task<IActionResult> CloneVacancyWithNewDates(CloneVacancyWithNewDatesEditModel model)
        {
            var response = await _orchestrator.PostCloneVacancyWithNewDates(model, User.ToVacancyUser());

            if(!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetDirtyCloneVacancyWithNewDatesViewModelAsync(model);
                return View(vm);
            }

            TempData.TryAdd(TempDataKeys.VacancyPreviewInfoMessage, InfoMessages.VacancyCloned);
            return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { VacancyId = response.Data, model.Ukprn });
        }        
    }
}