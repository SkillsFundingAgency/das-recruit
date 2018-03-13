using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class RoleDescriptionController : Controller
    {
        private readonly RoleDescriptionOrchestrator _orchestrator;

        public RoleDescriptionController(RoleDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("role-description", Name = RouteNames.RoleDescription_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("role-description", Name =  RouteNames.RoleDescription_Index_Post)]
        public async Task<IActionResult> Index(IndexEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetIndexViewModelAsync(m.VacancyId);
                return View(vm);
            }
            
            await _orchestrator.PostIndexEditModelAsync(m);

            return RedirectToRoute(RouteNames.CandidateProfile_Index_Get);
        }
    }
}