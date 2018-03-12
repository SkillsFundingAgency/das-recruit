using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
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