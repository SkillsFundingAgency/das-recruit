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
    [Route(RoutePaths.BlockedOrganisations)]
    public class BlockedOrganisationsController : Controller
    {
        private readonly BlockedOrganisationsOrchestrator _orchestrator;

        public BlockedOrganisationsController(BlockedOrganisationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.BlockedOrganisations_Get)]
        public async Task<IActionResult> Index()
        {
            var vm = await _orchestrator.GetBlockedOrganisationsViewModel();
            return View(vm);
        }

        [HttpGet("find-training-provider", Name = RouteNames.BlockProvider_Find_Get)]
        public IActionResult FindTrainingProvider()
        {
            if(TempData.ContainsKey(TempDataKeys.BlockProviderUkprnKey)) TempData.Remove(TempDataKeys.BlockProviderUkprnKey);
            return View(new FindTrainingProviderViewModel());
        }

        [HttpPost("find-training-provider", Name = RouteNames.BlockProvider_Find_Post)]
        public async Task<IActionResult> FindTrainingProvider(FindTrainingProviderEditModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(new FindTrainingProviderViewModel() { Ukprn = model.Ukprn, Postcode = model.Postcode });
            }

            long.TryParse(model.Ukprn, out var ukprn);
            var isBlocked = await _orchestrator.IsProviderAlreadyBlocked(ukprn);

            TempData[TempDataKeys.BlockProviderUkprnKey] = model.Ukprn;

            if(isBlocked)
            {
                return RedirectToRoute(RouteNames.BlockProvider_AlreadyBlocked_Get);
            }

            return RedirectToRoute(RouteNames.BlockProvider_Confirm_Get);
        }

        [HttpGet("already-blocked", Name = RouteNames.BlockProvider_AlreadyBlocked_Get)]
        public async Task<IActionResult> ProviderAlreadyBlocked()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();
            TempData.Remove(TempDataKeys.BlockProviderUkprnKey);

            var vm = await _orchestrator.GetProviderAlreadyBlockedViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpGet("confirm-blocking", Name = RouteNames.BlockProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProviderBlocking()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            var vm = await _orchestrator.GetConfirmTrainingProviderBlockingViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpPost("confirm-blocking", Name = RouteNames.BlockProvider_Confirm_Post)]
        public IActionResult ConfirmTrainingProviderBlocking_Post()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            return RedirectToRoute(RouteNames.BlockProvider_Consent_Get);
        }

        [HttpGet("consent-blocking", Name = RouteNames.BlockProvider_Consent_Get)]
        public async Task<IActionResult> ConsentForProviderBlocking()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            var vm = await _orchestrator.GetConsentForProviderBlockingViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpPost("consent-blocking", Name = RouteNames.BlockProvider_Consent_Post)]
        public async Task<IActionResult> ConsentForProviderBlocking(ConsentForProviderBlockingEditModel model)
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            if (ModelState.IsValid == false)
            {
                var vm = await _orchestrator.GetConsentForProviderBlockingViewModelAsync(ukprn);
                return View(vm);
            }
            
            await _orchestrator.BlockProviderAsync(ukprn, model.Reason, User.GetVacancyUser());

            return RedirectToRoute(RouteNames.BlockProvider_Acknowledgement_Get);
        }

        [HttpGet("blocking-acknowledgement", Name = RouteNames.BlockProvider_Acknowledgement_Get)]
        public async Task<IActionResult> ProviderBlockedAcknowledgement()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();
            TempData.Remove(TempDataKeys.BlockProviderUkprnKey);

            var vm = await _orchestrator.GetAcknowledgementViewModelAsync(ukprn);
            return View(vm);
        }
    }
}