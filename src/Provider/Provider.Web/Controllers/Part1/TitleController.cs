using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    public class TitleController : Controller
    {
        private const string VacancyTitleRoute = "vacancies/{vacancyId:guid}/title";
        private readonly TitleOrchestrator _orchestrator;

        public TitleController(TitleOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m, [FromQuery] bool wizard)
        {
             var response = await _orchestrator.PostTitleEditModelAsync(m, User.ToVacancyUser());

            // if (!response.Success)
            // {
            //     response.AddErrorsToModelState(ModelState);
            // }

            // if(!ModelState.IsValid)
            // {
            //     var vm = await _orchestrator.GetTitleViewModelAsync(m);
            //     vm.PageInfo.SetWizard(wizard);
            //     return View(vm);
            // }

            // return wizard
            //     ? RedirectToRoute(RouteNames.ShortDescription_Get, new {vacancyId = response.Data})
            //     : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}