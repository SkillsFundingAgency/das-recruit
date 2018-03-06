using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.LocationAndPositions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class LocationAndPositionsController : Controller
    {
        private readonly LocationAndPositionsOrchestrator _orchestrator;

        public LocationAndPositionsController(LocationAndPositionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("location-and-positions", Name = RouteNames.LocationAndPosition_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("location-and-positions", Name = RouteNames.LocationAndPosition_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.RoleDescription_Index_Get);
        }
    }
}