using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.CandidateProfile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class CandidateProfileController : Controller
    {
        private readonly CandidateProfileOrchestrator _orchestrator;

        public CandidateProfileController(CandidateProfileOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("candidate-profile", Name = RouteNames.CandidateProfile_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }
                
        [HttpPost("candidate-profile", Name = RouteNames.CandidateProfile_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.ApprenticeshipDetails_Index_Get);
        }
    }
}