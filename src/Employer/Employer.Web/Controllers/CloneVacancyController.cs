using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.CloneVacancy;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class CloneVacancyController : Controller
    {
        private readonly CloneVacancyOrchestrator _orchestrator;
        public CloneVacancyController(CloneVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("clone", Name = RouteNames.CloneVacancy_Get)]
        public async Task<IActionResult> Clone(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetCloneableAuthorisedVacancyAsync(vrm);

            return _orchestrator.IsNewDatesRequired(vacancy) 
                ? RedirectToRoute(RouteNames.CloneVacancyWithNewDates_Get) 
                : RedirectToRoute(RouteNames.CloneVacancyDatesQuestion_Get);
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
                TempData.TryAdd(TempDataKeys.VacancyClonedInfoMessage, InfoMessages.AdvertCloned);
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { VacancyId = newVacancyId });
            }
            else
            {
                return RedirectToRoute(RouteNames.CloneVacancyWithNewDates_Get);
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

            TempData.TryAdd(TempDataKeys.VacancyClonedInfoMessage, InfoMessages.AdvertCloned);
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { VacancyId = response.Data });
        }
    }
}