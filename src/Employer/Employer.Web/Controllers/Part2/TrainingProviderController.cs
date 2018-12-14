using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingProviderController : Controller
    {
        private readonly TrainingProviderOrchestrator _orchestrator;
        private readonly IEmployerVacancyClient _client;
        private const string TrainingProviderJourneyTempDataKey = "FromSelectTrainingProvider";
        private const string InvalidUkprnMessageFormat = "The UKPRN {0} is not valid or the associated provider is not active.";

        public TrainingProviderController(TrainingProviderOrchestrator orchestrator, IEmployerVacancyClient client)
        {
            _orchestrator = orchestrator;
            _client = client;
        }

        [HttpGet("select-training-provider", Name = RouteNames.TrainingProvider_Select_Get)]
        public async Task<IActionResult> SelectTrainingProvider(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetSelectTrainingProviderViewModel(vrm);
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

            if (long.TryParse(m.Ukprn, out var ukprnAsLong) == false)
                return await ProviderNotFound(m);

            var providerExists = await _orchestrator.ConfirmProviderExists(ukprnAsLong);
            
            if (providerExists == false)
                return await ProviderNotFound(m);

            var confirmDetailsVm = await _orchestrator.GetConfirmViewModel(m);
            TempData.Add(TrainingProviderJourneyTempDataKey, 1);
            return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get, confirmDetailsVm);
        }

        [HttpGet("confirm-training-provider", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public async Task<IActionResult> ConfirmTrainingProvider(ConfirmTrainingProviderViewModel confirmDetailsVm)
        {
            if (!TempData.ContainsKey(TrainingProviderJourneyTempDataKey))
                return RedirectToRoute(RouteNames.TrainingProvider_Select_Get);

            TempData.Remove(TrainingProviderJourneyTempDataKey);
            var vacancy = await _client.GetVacancyAsync(confirmDetailsVm.VacancyId);
            Utility.CheckAuthorisedAccess(vacancy, confirmDetailsVm.EmployerAccountId);
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
            ModelState.AddModelError(string.Empty, string.Format(InvalidUkprnMessageFormat, m.Ukprn));
            var vm = await _orchestrator.GetSelectTrainingProviderViewModel(m);
            return View(ViewNames.SelectTrainingProvider, vm);
        }
    }
}