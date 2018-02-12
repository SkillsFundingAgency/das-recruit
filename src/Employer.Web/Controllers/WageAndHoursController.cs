using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.WageAndHours;

namespace Employer.Web.Controllers
{
    public class WageAndHoursController : Controller
    {
        [HttpGet, Route("wage-and-hours")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("wage-and-hours")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "ApplicationProcess");
        }
    }
}