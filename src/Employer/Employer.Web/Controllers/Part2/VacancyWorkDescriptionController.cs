using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyWorkDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
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
                return View(vm);
            }
            
            if (vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
            }
            return RedirectToRoute(RouteNames.EmployerTaskListGet, new {m.VacancyId, m.EmployerAccountId});
        }
    }
}