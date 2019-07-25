using System.ComponentModel;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Models.WithdrawVacancy;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
    [Route(RoutePaths.WithdrawVacancyPath)]
    public class WithdrawVacancyController : Controller
    {
        private readonly WithdrawVacancyOrchestrator _orchestrator;

        public WithdrawVacancyController(WithdrawVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet(Name = RouteNames.WithdrawVacancy_FindVacancy_Get)]
        public IActionResult FindVacancy()
        {
            var vm = _orchestrator.GetFindVacancyViewModel();

            return View(vm);
        }

        [HttpPost(Name = RouteNames.WithdrawVacancy_FindVacancy_Post)]
        public async Task<IActionResult> FindVacancy(FindVacancyEditModel m)
        {
            if (ModelState.IsValid)
            {
                var result = await _orchestrator.PostFindVacancyEditModelAsync(m);

                switch (result.ResultType)
                {
                    case PostFindVacancyEditModelResultType.AlreadyClosed:
                        return RedirectToRoute(RouteNames.WithdrawVacancy_AlreadyClosed_Get, new {vacancyReference = result.VacancyReference});
                    case PostFindVacancyEditModelResultType.CanClose:
                        return RedirectToRoute(RouteNames.WithdrawVacancy_Confirm_Get, new {vacancyReference = result.VacancyReference});
                    case PostFindVacancyEditModelResultType.NotFound:
                        ModelState.AddModelError(nameof(FindVacancyViewModel.VacancyReference), $"Cannot find a live vacancy with vacancy reference '{m.VacancyReference}'");
                        break;
                    default:
                        throw new InvalidEnumArgumentException($"{result.ResultType.ToString()} is not handled");
                }
            }

            var vm = _orchestrator.GetFindVacancyViewModel(m);
            return View(vm);
        }

        [HttpGet("{vacancyReference}/already-closed", Name = RouteNames.WithdrawVacancy_AlreadyClosed_Get)]
        public async Task<IActionResult> AlreadyClosed([FromRoute] string vacancyReference)
        {
            var vm = await _orchestrator.GetAlreadyClosedViewModelAsync(vacancyReference);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpGet("{vacancyReference}/confirm", Name = RouteNames.WithdrawVacancy_Confirm_Get)]
        public async Task<IActionResult> Confirm([FromRoute] string vacancyReference)
        {
            var vm = await _orchestrator.GetConfirmViewModelAsync(vacancyReference);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpGet("{vacancyReference}/acknowledge", Name = RouteNames.WithdrawVacancy_Acknowledge_Get)]
        public async Task<IActionResult> Acknowledge([FromRoute] string vacancyReference)
        {
            var vm = await _orchestrator.GetAcknowledgeViewModelAsync(vacancyReference);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpPost("{vacancyReference}/acknowledge", Name = RouteNames.WithdrawVacancy_Acknowledge_Post)]
        public async Task<IActionResult> Acknowledge(AcknowledgeEditModel m)
        {
            if (ModelState.IsValid)
            {
                var success = await _orchestrator.PostAcknowledgeEditModelAsync(m, User.GetVacancyUser());

                if (success)
                    return RedirectToRoute(RouteNames.WithdrawVacancy_Closed_Get);

                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);
            }

            var vm = await _orchestrator.GetAcknowledgeViewModelAsync(m.VacancyReference);
            return View(vm);
        }

        [HttpGet("{vacancyReference}/closed", Name = RouteNames.WithdrawVacancy_Closed_Get)]
        public async Task<IActionResult> Closed([FromRoute] string vacancyReference)
        {
            var vm = await _orchestrator.GetClosedViewModelAsync(vacancyReference);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }
    }
}
