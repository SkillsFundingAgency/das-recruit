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
    [Route(RoutePaths.ManageProvider)]
    public class ManageProviderController : Controller
    {
        private readonly ManageProviderOrchestrator _orchestrator;
        private readonly UserAuthorizationService _userAuthorizationService;
        public ManageProviderController(UserAuthorizationService userAuthorizationService, ManageProviderOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
            _userAuthorizationService = userAuthorizationService;
        }

        [HttpGet("", Name = RouteNames.RemoveTrainingProvider_Get)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("find-training-provider", Name = RouteNames.FindTrainingProvider_Get)]
        public IActionResult FindTrainingProvider()
        {
            if(TempData.ContainsKey(TempDataKeys.BlockProviderUkprnKey)) TempData.Remove(TempDataKeys.BlockProviderUkprnKey);
            return View(new FindTrainingProviderViewModel());
        }

        [HttpPost("find-training-provider", Name = RouteNames.FindTrainingProvider_Post)]
        public async Task<IActionResult> FindTrainingProvider(FindTrainingProviderEditModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(new FindTrainingProviderViewModel() { Ukprn = model.Ukprn, Postcode = model.Postcode });
            }
            var isBlocked = await _orchestrator.IsProviderAlreadyBlocked(model.Ukprn.GetValueOrDefault());

            TempData[TempDataKeys.BlockProviderUkprnKey] = model.Ukprn;

            if(isBlocked)
            {
                return RedirectToRoute(RouteNames.ProviderAlreadyBlocked_Get);
            }

            return RedirectToRoute(RouteNames.ConfirmProviderBlocking_Get);
        }

        [HttpGet("already-blocked", Name = RouteNames.ProviderAlreadyBlocked_Get)]
        public async Task<IActionResult> ProviderAlreadyBlocked()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();
            TempData.Remove(TempDataKeys.BlockProviderUkprnKey);

            var vm = await _orchestrator.GetProviderAlreadyBlockedViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpGet("confirm-blocking", Name = RouteNames.ConfirmProviderBlocking_Get)]
        public async Task<IActionResult> ConfirmTrainingProviderBlocking()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            var vm = await _orchestrator.GetConfirmTrainingProviderBlockingViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpPost("confirm-blocking", Name = RouteNames.ConfirmProviderBlocking_Post)]
        public IActionResult ConfirmTrainingProviderBlocking_Post()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            return RedirectToRoute(RouteNames.ConsentForProviderBlocking_Get);
        }

        [HttpGet("consent-blocking", Name = RouteNames.ConsentForProviderBlocking_Get)]
        public async Task<IActionResult> ConsentForProviderBlocking()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();

            var vm = await _orchestrator.GetConsentForProviderBlockingViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpPost("consent-blocking", Name = RouteNames.ConsentForProviderBlocking_Post)]
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

            return RedirectToRoute(RouteNames.ProviderBlockedAcknowledgement_Get);
        }

        [HttpGet("blocking-acknowledgement", Name = RouteNames.ProviderBlockedAcknowledgement_Get)]
        public async Task<IActionResult> ProviderBlockedAcknowledgement()
        {
            var data = TempData.Peek(TempDataKeys.BlockProviderUkprnKey)?.ToString();
            if(long.TryParse(data, out var ukprn) == false) return BadRequest();
            TempData.Remove(TempDataKeys.BlockProviderUkprnKey);

            var vm = await _orchestrator.GetAcknowledgementViewModelAsync(ukprn);
            return View(vm);
        }

        [HttpGet("blocked-organisations", Name = RouteNames.BlockedOrganisations_Get)]
        public async Task<IActionResult> BlockedOrganisations()
        {
            var vm = await _orchestrator.GetBlockedOrganisationsViewModel();
            return View(vm);
        }
    }
}