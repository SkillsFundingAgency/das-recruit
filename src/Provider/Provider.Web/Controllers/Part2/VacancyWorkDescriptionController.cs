using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyWorkDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyWorkDescriptionController : Controller
    {
        private readonly VacancyWorkDescriptionOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public VacancyWorkDescriptionController(VacancyWorkDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-work-description", Name = RouteNames.VacancyWorkDescription_Index_Get)]
        public async Task<IActionResult> VacancyWorkDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("vacancy-work-description", Name =  RouteNames.VacancyWorkDescription_Index_Post)]
        public async Task<IActionResult> VacancyWorkDescription(VacancyWorkDescriptionEditModel m)
        {
            var response = await _orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                vm.VacancyDescription = m.VacancyDescription;
                return View(vm);
            }
            
            if (vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.VacancyId, m.Ukprn});
            }
            return RedirectToRoute(RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get, new {m.VacancyId, m.Ukprn});
        }
    }
}