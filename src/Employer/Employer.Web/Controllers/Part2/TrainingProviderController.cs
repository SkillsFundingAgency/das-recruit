using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active.";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("select-training-provider", Name = RouteNames.TrainingProvider_Select_Get)]
        public async Task<IActionResult> SelectTrainingProvider(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModel(vacancyId);
            return View(vm);
        }

        [HttpPost("select-training-provider", Name = RouteNames.TrainingProvider_Select_Post)]
        public async Task<IActionResult> SelectTrainingProvider(SelectTrainingProviderEditModel m)
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
            return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, confirmDetailsVm);
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public IActionResult ConfirmTrainingProvider(ConfirmTrainingProviderViewModel confirmDetailsVm)
        {
            return View(confirmDetailsVm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> ConfirmTrainingProvider(ConfirmTrainingProviderEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var providerExists = await _orchestrator.ConfirmProviderExists(long.Parse(m.Ukprn));

            if (providerExists == false)
            {
                var vm = new SelectTrainingProviderEditModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn };
                return await ProviderNotFound(vm);
            }

            await _orchestrator.PostConfirmEditModelAsync(m);
            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }

        private async Task<IActionResult> ProviderNotFound(SelectTrainingProviderEditModel m)
        {
            ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
            var vm = await _orchestrator.GetIndexViewModel(m.VacancyId);
            return View("SelectTrainingProvider", vm);
        }
    }
}