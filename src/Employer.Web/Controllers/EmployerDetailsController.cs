using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.EmployerDetails;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class EmployerDetailsController : Controller
    {
        [HttpGet, Route("employer-details", Name = RouteNames.EmployerDetails_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("employer-details", Name = RouteNames.EmployerDetails_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "LocationAndPositions");
        }
    }
}