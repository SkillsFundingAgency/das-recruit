using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active.";
        private const string InvalidSearchTerm = "Please enter a training provider name or UKPRN";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator, IRecruitVacancyClient vacancyClient)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("select-training-provider", Name = RouteNames.TrainingProvider_Select_Get)]
        public async Task<IActionResult> SelectTrainingProvider(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] long? ukprn = null)
        {
            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(vrm, ukprn);
            vm.PageInfo.SetWizard(wizard);

            if (vm.IsTrainingProviderSelected.GetValueOrDefault() && !string.IsNullOrWhiteSpace(vm.Ukprn))
                return GetRedirectToNextPage(Convert.ToBoolean(wizard));

            if (string.IsNullOrWhiteSpace(clear) == false)
            {
                vm.Ukprn = string.Empty;
                vm.TrainingProviderSearch = string.Empty;
                vm.IsTrainingProviderSelected = true;
            }
            
            return View(vm);
        }

        [HttpPost("select-training-provider", Name = RouteNames.TrainingProvider_Select_Post)]
        public async Task<IActionResult> SelectTrainingProvider(SelectTrainingProviderEditModel m, [FromQuery] bool wizard)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            var result = await _orchestrator.PostSelectTrainingProviderAsync(m, User.ToVacancyUser());

            switch (result.ResponseType)
            {
                case SelectTrainingProviderResponseType.Continue:
                    return GetRedirectToNextPage(wizard);
                case SelectTrainingProviderResponseType.NotFound:
                    return await ProviderNotFound(m, wizard);
                default:
                    return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, new { ukprn = result.FoundProviderUkprn, wizard });
            }
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProvider(VacancyRouteModel vrm, [FromQuery] string ukprn, [FromQuery] string wizard)
        {
            var provider = await _orchestrator.GetProviderAsync(ukprn);
            
            if(provider == null)
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);
            
            var vm = await _orchestrator.GetConfirmViewModelAsync(vrm, provider.Ukprn);
            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        [HttpPost("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public async Task<IActionResult> ConfirmTrainingProvider(ConfirmTrainingProviderEditModel m, [FromQuery] bool wizard)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var provider = await _orchestrator.GetProviderAsync(m.Ukprn);

            if (provider == null)
            {
                var vm = new SelectTrainingProviderEditModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn, SelectionType = TrainingProviderSelectionType.Ukprn};
                return await ProviderNotFound(vm, wizard);
            }

            var response = await _orchestrator.PostConfirmEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
                return View(ViewNames.SelectTrainingProvider, vm);
            }

            return GetRedirectToNextPage(wizard);
        }

        private async Task<IActionResult> ProviderNotFound(SelectTrainingProviderEditModel m, bool wizard)
        {
            if (m.SelectionType == TrainingProviderSelectionType.Ukprn)
            {
                ModelState.AddModelError(nameof(SelectTrainingProviderEditModel.Ukprn), string.Format(InvalidUkprnMessageFormat, m.Ukprn));
            }
            else
            {
                ModelState.AddModelError(nameof(SelectTrainingProviderEditModel.TrainingProviderSearch), InvalidSearchTerm);
            }

            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
            vm.PageInfo.SetWizard(wizard);
            return View(ViewNames.SelectTrainingProvider, vm);
        }

        private IActionResult GetRedirectToNextPage(bool wizard)
        {
            return wizard
                ? RedirectToRoute(RouteNames.NumberOfPositions_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, Anchors.TrainingProviderSection);
        }
    }
}