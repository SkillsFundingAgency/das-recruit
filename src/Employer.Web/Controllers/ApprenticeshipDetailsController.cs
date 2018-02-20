using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class ApprenticeshipDetailsController : Controller
    {
        private readonly ApprenticeshipDetailsOrchestrator _orchestrator;

        public ApprenticeshipDetailsController(ApprenticeshipDetailsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.TrainingProvider_Index_Get);
        }
    }
}