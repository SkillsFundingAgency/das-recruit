using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.CandidateProfile;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class CandidateProfileController : Controller
    {
        [HttpGet, Route("candidate-profile")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("candidate-profile")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "ApprenticeshipDetails");
        }
    }
}