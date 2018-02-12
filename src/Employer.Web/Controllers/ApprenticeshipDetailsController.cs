using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class ApprenticeshipDetailsController : Controller
    {
        [HttpGet, Route("apprenticeship-details")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("apprenticeship-details")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "TrainingProvider");
        }
    }
}