using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.TrainingProvider;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
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
        public async Task<IActionResult> SelectTrainingProvider(VacancyRouteModel vrm, [FromQuery] string clear = "")
        {
            var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(vrm);

            if (string.IsNullOrWhiteSpace(clear) == false)
            {
                vm.Ukprn = string.Empty;
                vm.TrainingProviderSearch = string.Empty;
                vm.SelectTrainingProvider = true;
            }
            
            return View(vm);
        }

        [HttpPost("select-training-provider", Name = RouteNames.TrainingProvider_Select_Post)]
        public async Task<IActionResult> SelectTrainingProvider(SelectTrainingProviderEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
                return View(vm);
            }

            var result = await _orchestrator.PostSelectTrainingProviderAsync(m, User.ToVacancyUser());

            switch (result.Action)
            {
                case PostSelectTrainingProviderResultAction.TrainingProviderContinue:
                    return RedirectToRoute(RouteNames.Vacancy_Preview_Get, Anchors.TrainingProviderSection);
                case PostSelectTrainingProviderResultAction.TrainingProviderNotFound:
                    return await ProviderNotFound(m);
                default:
                    return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, new { ukprn = result.FoundProviderUkprn });
            }
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProvider(VacancyRouteModel vrm, [FromQuery] string ukprn)
        {
            var provider = await _orchestrator.GetProviderAsync(ukprn);
            
            if(provider == null)
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);
            
            var confirmDetailsVm = await _orchestrator.GetConfirmViewModelAsync(vrm, provider.Ukprn);

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
                var vm = await _orchestrator.GetSelectTrainingProviderViewModelAsync(m);
                return View(ViewNames.SelectTrainingProvider, vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, Anchors.TrainingProviderSection);
        }

        private async Task<IActionResult> ProviderNotFound(SelectTrainingProviderEditModel m)
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
            return View(ViewNames.SelectTrainingProvider, vm);
        }
    }
}