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
        private const string InvalidUkprnMessageFormat = "This UKPRN {0} is not valid or the associated provider is not active.";

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
        public async Task<IActionResult> Index(IndexEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetIndexViewModelAsync(m.VacancyId);
                return View(vm);
            }

            var providerExists = await _orchestrator.ConfirmProviderExists(long.Parse(m.Ukprn));
            
            if (providerExists == false)
            {
                ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
                var vm = await _orchestrator.GetIndexViewModelAsync(m.VacancyId);
                return View(vm);
            }

            var confirmDetailsVm = await _orchestrator.PostIndexEditModelAsync(m);
            return View("Confirm", confirmDetailsVm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> Confirm(ConfirmEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var providerExists = await _orchestrator.ConfirmProviderExists(long.Parse(m.Ukprn));

            if (providerExists == false)
            {
                ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
                return RedirectToRoute(RouteNames.TrainingProvider_Index_Get, new { m.VacancyId });
            }

            await _orchestrator.PostConfirmEditModelAsync(m);
            return RedirectToRoute(RouteNames.WageAndhours_Index_Get);
        }
    }
}