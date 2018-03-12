using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active.";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator)

        {
            _orchestrator = orchestrator;
        }

        [HttpGet("training-provider", Name = RouteNames.TrainingProvider_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModel(vacancyId);
            return View(vm);
        }

        [HttpPost("training-provider", Name = RouteNames.TrainingProvider_Index_Post)]
        public async Task<IActionResult> Index(IndexEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetIndexViewModel(m.VacancyId);
                return View(vm);
            }

            var providerExists = await _orchestrator.ConfirmProviderExists(long.Parse(m.Ukprn));
            
            if (providerExists == false)
                return await ProviderNotFound(m);

            var confirmDetailsVm = await _orchestrator.GetConfirmViewModel(m);
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
                var vm = new IndexEditModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn };
                return await ProviderNotFound(vm);
            }

            await _orchestrator.PostConfirmEditModelAsync(m);
            return RedirectToRoute(RouteNames.WageAndhours_Index_Get);
        }

        private async Task<IActionResult> ProviderNotFound(IndexEditModel m)
        {
            ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
            var vm = await _orchestrator.GetIndexViewModel(m.VacancyId);
            return View("Index", vm);
        }
    }
}