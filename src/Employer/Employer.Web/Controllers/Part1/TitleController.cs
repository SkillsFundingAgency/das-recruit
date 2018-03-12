using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TitleVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route("accounts/{employerAccountId:minlength(6)}")]
    public class TitleController : Controller
    {
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

        [HttpGet("vacancies/{vacancyId:guid:validGuid}/title", Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(Guid vacancyId)
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost("vacancies/{vacancyId:guid:validGuid}/title", Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            
            var vacancyId = await _orchestrator.PostTitleViewModelAsync(vm);
            
            return RedirectToRoute(RouteNames.Location_Get, new { vacancyId });
        }
    }
}