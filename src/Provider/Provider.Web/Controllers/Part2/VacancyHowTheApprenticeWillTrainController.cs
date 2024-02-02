using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyHowTheApprenticeWillTrain;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyHowTheApprenticeWillTrainController : Controller
    {
        private readonly VacancyHowWillTheApprenticeTrainOrchestrator _orchestrator;

        public VacancyHowTheApprenticeWillTrainController(VacancyHowWillTheApprenticeTrainOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("how-will-the-apprentice-train", Name = RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get)]
        public async Task<IActionResult> VacancyHowTheApprenticeWillTrain(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("how-will-the-apprentice-train", Name =  RouteNames.VacancyHowTheApprenticeWillTrain_Index_Post)]
        public async Task<IActionResult> VacancyHowTheApprenticeWillTrain(VacancyHowTheApprenticeWillTrainEditModel m)
        {
            var response = await _orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            if (vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.VacancyId, m.Ukprn});
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {m.VacancyId, m.Ukprn});
        }
    }
    
}