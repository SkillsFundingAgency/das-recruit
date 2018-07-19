using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Employer.Web.Views;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class TitleController : Controller
    {
        private const string VacancyTitleRoute = "vacancies/{vacancyId:guid}/title";
        private readonly TitleOrchestrator _orchestrator;

        public TitleController(TitleOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("create-vacancy", Name = RouteNames.CreateVacancy_Get)]
        public IActionResult Title()
        {
            var vm = _orchestrator.GetTitleViewModel();
            vm.IsWizard = true;
            return View(vm);
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, bool wizard = true)
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            vm.IsWizard = wizard;
            return View(vm);
        }
        
        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m, bool wizard = true)
        {
            var response = await _orchestrator.PostTitleEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTitleViewModelAsync(m);
                vm.IsWizard = wizard;
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.ShortDescription_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, PreviewAnchors.TitleSection);
        }
    }
}