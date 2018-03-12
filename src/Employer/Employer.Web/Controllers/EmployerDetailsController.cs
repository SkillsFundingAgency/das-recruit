using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.EmployerDetails;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class EmployerDetailsController : Controller
    {
        private readonly EmployerDetailsOrchestrator _orchestrator;

        public EmployerDetailsController(EmployerDetailsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("employer-details", Name = RouteNames.EmployerDetails_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("employer-details", Name = RouteNames.EmployerDetails_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.Location_Get);
        }
    }
}