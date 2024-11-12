using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class EmployerController : Controller
    {
        private readonly EmployerOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public EmployerController(EmployerOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel)
        {
            var vm = await _orchestrator.GetEmployersViewModelAsync(vacancyRouteModel);
            return View(vm);
        }

        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel, EmployersEditModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SelectedEmployerId))
            {
                ModelState.AddModelError(nameof(model.SelectedEmployerId), ValidationMessages.EmployerSelectionMessages.EmployerMustBeSelectedMessage);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployersViewModelAsync(model);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.ProviderTaskListCreateGet, new {employerAccountId = model.SelectedEmployerId, vacancyRouteModel.Ukprn});
        }
    }
}