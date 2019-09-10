using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using System.Threading.Tasks;
using Esfa.Recruit.QA.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.QA.Web.ViewModels.ManageProvider;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Extensions;

namespace Esfa.Recruit.QA.Web.Controllers
{
    [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
    [Route(RoutePaths.UnBlockOrganisationRoutePath)]
    public class UnBlockOrganisationController : Controller
    {
        private readonly UnBlockOrganisationOrchestrator _orchestrator;

        public UnBlockOrganisationController(UnBlockOrganisationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.UnBlockProvider_Confirm_Get)]
        public async Task<IActionResult> RestoreAccess(string organisationId)
        {
            if (long.TryParse(organisationId, out var ukprn) == false) return BadRequest();
            var vm = await _orchestrator.GetBlockedOrganisationViewModel(ukprn);
            return View(vm);
        }

        [HttpPost("", Name = RouteNames.UnBlockProvider_Confirm_Post)]
        public async Task<IActionResult> RestoreAccess(UnBlockTrainingProviderEditModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(new UnBlockTrainingProviderEditModel() { Ukprn = model.Ukprn, ProviderName = model.ProviderName });
            }
            TempData[TempDataKeys.UnBlockedProviderUkprnKey] = model.Ukprn;
            var isBlocked = await _orchestrator.IsProviderAlreadyBlocked(model.Ukprn);
            if (!isBlocked)
            {
                return View(new UnBlockTrainingProviderEditModel() { Ukprn = model.Ukprn, ProviderName = model.ProviderName });
            }

            if (model.CanRestoreAccess)
            {
                await _orchestrator.UnBlockProviderAsync(model.Ukprn, User.GetVacancyUser());
                return RedirectToRoute(RouteNames.UnBlockProvider_Acknowledgement_Get);
            }
            return RedirectToRoute(RouteNames.BlockedOrganisations_Get);
        }

        [HttpGet("unblocking-acknowledgement", Name = RouteNames.UnBlockProvider_Acknowledgement_Get)]
        public async Task<IActionResult> ProviderUnBlockedAcknowledgement()
        {
            var data = TempData.Peek(TempDataKeys.UnBlockedProviderUkprnKey)?.ToString();
            if (long.TryParse(data, out var ukprn) == false) return BadRequest();
            TempData.Remove(TempDataKeys.UnBlockedProviderUkprnKey);

            var vm = await _orchestrator.GetAcknowledgementViewModelAsync(ukprn);
            return View(vm);
        }
    }
}