using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.CandidateProfile;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class CandidateProfileController : Controller
    {
        [HttpGet("candidate-profile", Name = RouteNames.CandidateProfile_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }
                
        [HttpPost("candidate-profile", Name = RouteNames.CandidateProfile_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.ApprenticeshipDetails_Index_Get);
        }
    }
}