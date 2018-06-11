using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class AboutEmployerController : Controller
    {
        private readonly AboutEmployerOrchestrator _orchestrator;

        public AboutEmployerController(AboutEmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("about-employer", Name = RouteNames.AboutEmployer_Get)]
        public async Task<IActionResult> AboutEmployer(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetAboutEmployerViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("about-employer", Name =  RouteNames.AboutEmployer_Post)]
        public async Task<IActionResult> AboutEmployer(AboutEmployerEditModel m)
        {
            var response = await _orchestrator.PostAboutEmployerEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetAboutEmployerViewModelAsync(m);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, AboutEmployerViewModel.PreviewSectionAnchor);
        }
    }
}