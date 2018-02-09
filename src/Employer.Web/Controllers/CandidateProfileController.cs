using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.CandidateProfile;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class CandidateProfileController : Controller
    {
        [HttpGet, Route("accounts/{employerAccountId}/candidate-profile", Name = RouteNames.CandidateProfile_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }
                
        [HttpPost, Route("accounts/{employerAccountId}/candidate-profile", Name = RouteNames.CandidateProfile_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "ApprenticeshipDetails");
        }
    }
}