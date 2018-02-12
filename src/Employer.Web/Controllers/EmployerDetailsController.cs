using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.EmployerDetails;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class EmployerDetailsController : Controller
    {
        [HttpGet, Route("employer-details")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("employer-details")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "LocationAndPositions");
        }
    }
}