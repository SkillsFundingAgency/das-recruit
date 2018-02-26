using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("training-provider", Name = RouteNames.TrainingProvider_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("training-provider", Name = RouteNames.TrainingProvider_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get);
        }

        [HttpGet("training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> Confirm(Guid vacancyId)
        {
            var vm = await _orchestrator.GetConfirmViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public IActionResult Confirm(ConfirmViewModel vm)
        {
            return RedirectToRoute(RouteNames.WageAndhours_Index_Get);
        }
    }
}