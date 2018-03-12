using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.WageAndHours;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class WageAndHoursController : Controller
    {
        private readonly WageAndHoursOrchestrator _orchestrator;

        public WageAndHoursController(WageAndHoursOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("wage-and-hours", Name = RouteNames.WageAndhours_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("wage-and-hours", Name = RouteNames.WageAndhours_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.ApplicationProcess_Index_Get);
        }
    }
}