using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration;
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

                TempData[TempDataKeys.WithdrawVacancyReference] = result.VacancyReference;

                switch (result.ResultType)
                {
                    case PostFindVacancyEditModelResultType.AlreadyClosed:
                        return RedirectToRoute(RouteNames.WithdrawVacancy_AlreadyClosed_Get, result.VacancyReference.Value);
                    case PostFindVacancyEditModelResultType.CanClose:
                        return RedirectToRoute(RouteNames.WithdrawVacancy_Confirm_Get, result.VacancyReference.Value);
                    case PostFindVacancyEditModelResultType.NotFound:
                        ModelState.AddModelError(nameof(FindVacancyViewModel.VacancyReference), $"There are no live vacancies with the reference '{m.VacancyReference}'");
                        break;
                    case PostFindVacancyEditModelResultType.CannotClose:
                        ModelState.AddModelError(nameof(FindVacancyViewModel.VacancyReference), $"There are no live vacancies with the reference 'VAC{result.VacancyReference}'");
                        break;
                    default:
                        throw new NotImplementedException($"{result.ResultType.ToString()} is not handled");
                }
            }

            var vm = _orchestrator.GetFindVacancyViewModel(m);
            return View(vm);
        }

        [HttpGet("already-closed", Name = RouteNames.WithdrawVacancy_AlreadyClosed_Get)]
        public async Task<IActionResult> AlreadyClosed()
        {
            var vacancyReference = GetVacancyReference();

            AlreadyClosedViewModel vm = null;

            if (vacancyReference.HasValue)
                vm = await _orchestrator.GetAlreadyClosedViewModelAsync(vacancyReference.Value);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpGet("confirm", Name = RouteNames.WithdrawVacancy_Confirm_Get)]
        public async Task<IActionResult> Confirm()
        {
            var vacancyReference = GetVacancyReference();

            ConfirmViewModel vm = null;

            if(vacancyReference.HasValue)
                vm = await _orchestrator.GetConfirmViewModelAsync(vacancyReference.Value);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpGet("consent", Name = RouteNames.WithdrawVacancy_Consent_Get)]
        public async Task<IActionResult> Consent()
        {
            var vacancyReference = GetVacancyReference();

            ConsentViewModel vm = null;

            if (vacancyReference.HasValue)
                vm = await _orchestrator.GetConsentViewModelAsync(vacancyReference.Value);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            return View(vm);
        }

        [HttpPost("consent", Name = RouteNames.WithdrawVacancy_Consent_Post)]
        public async Task<IActionResult> Consent(ConsentEditModel m)
        {
            var vacancyReference = GetVacancyReference();

            if(vacancyReference.HasValue == false)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            if (ModelState.IsValid)
            {
                var success = await _orchestrator.PostConsentEditModelAsync(m, vacancyReference.Value, User.GetVacancyUser());

                if (success)
                    return RedirectToRoute(RouteNames.WithdrawVacancy_Acknowledgement_Get);

                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);
            }

            var vm = await _orchestrator.GetConsentViewModelAsync(vacancyReference.Value);
            return View(vm);
        }

        [HttpGet("acknowledgement", Name = RouteNames.WithdrawVacancy_Acknowledgement_Get)]
        public async Task<IActionResult> Acknowledgement()
        {
            var vacancyReference = GetVacancyReference();

            AcknowledgementViewModel vm = null;

            if (vacancyReference.HasValue)
                vm = await _orchestrator.GetAcknowledgementViewModelAsync(vacancyReference.Value);

            if (vm == null)
                return RedirectToRoute(RouteNames.WithdrawVacancy_FindVacancy_Get);

            TempData.Remove(TempDataKeys.WithdrawVacancyReference);

            return View(vm);
        }

        private long? GetVacancyReference()
        {
            if (long.TryParse(TempData.Peek(TempDataKeys.WithdrawVacancyReference)?.ToString(), out long vacancyReferenceNumber))
                return vacancyReferenceNumber;
            return null;
        }
    }
}
