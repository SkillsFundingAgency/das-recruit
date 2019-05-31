﻿using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.TrainingProvider;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private readonly IRecruitVacancyClient _vacancyClient;
        private const string TrainingProviderJourneyTempDataKey = "FromSelectTrainingProvider";
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active.";
        private const string InvalidSearchTerm = "Please enter a training provider name or UKPRN";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator, IRecruitVacancyClient vacancyClient)
        {
            _orchestrator = orchestrator;
            _vacancyClient = vacancyClient;
        }

        [HttpGet("select-training-provider", Name = RouteNames.TrainingProvider_Select_Get)]
        public async Task<IActionResult> SelectTrainingProvider(VacancyRouteModel vrm, [FromQuery] string clear = "")
        {
            var vm = await _orchestrator.GetSelectTrainingProviderViewModel(vrm);

            if (string.IsNullOrWhiteSpace(clear) == false)
            {
                vm.Ukprn = string.Empty;
                vm.TrainingProviderSearch = string.Empty;
            }
            
            return View(vm);
        }

        [HttpPost("select-training-provider", Name = RouteNames.TrainingProvider_Select_Post)]
        public async Task<IActionResult> SelectTrainingProvider(SelectTrainingProviderEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSelectTrainingProviderViewModel(m);
                return View(vm);
            }

            var provider = await _orchestrator.GetProviderFromModelAsync(m);

            if (provider == null)
                return await ProviderNotFound(m);

            TempData.Add(TrainingProviderJourneyTempDataKey, 1);
            return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, new {ukprn = provider.Ukprn});
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProvider(VacancyRouteModel vrm, [FromQuery] string ukprn)
        {
            if (!TempData.ContainsKey(TrainingProviderJourneyTempDataKey))
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);

            TempData.Remove(TrainingProviderJourneyTempDataKey);

            var provider = await _orchestrator.GetProviderAsync(ukprn);
            
            if(provider == null)
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);
            
            var confirmDetailsVm = await _orchestrator.GetConfirmViewModel(vrm, provider.Ukprn);

            return View(confirmDetailsVm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> ConfirmTrainingProvider(ConfirmTrainingProviderEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var provider = await _orchestrator.GetProviderAsync(m.Ukprn);

            if (provider == null)
            {
                var vm = new SelectTrainingProviderEditModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn, SelectionType = TrainingProviderSelectionType.Ukprn};
                return await ProviderNotFound(vm);
            }

            var response = await _orchestrator.PostConfirmEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSelectTrainingProviderViewModel(m);
                return View(ViewNames.SelectTrainingProvider, vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private async Task<IActionResult> ProviderNotFound(SelectTrainingProviderEditModel m)
        {
            if (m.SelectionType == TrainingProviderSelectionType.Ukprn)
            {
                ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
            }
            else
            {
                ModelState.AddModelError(string.Empty, InvalidSearchTerm);
            }
            var vm = await _orchestrator.GetSelectTrainingProviderViewModel(m);
            return View(ViewNames.SelectTrainingProvider, vm);
        }
    }
}