using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
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
            return View(vm);
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(Guid vacancyId)
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vacancyId);
            return View(vm);
        }
        
        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTitleViewModelAsync(m);
                return View(vm);
            }
            
            var vacancyId = await _orchestrator.PostTitleEditModelAsync(m);
            
            return RedirectToRoute(RouteNames.Location_Get, new { vacancyId });
        }
    }
}