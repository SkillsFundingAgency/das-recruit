using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.LocationAndPositions;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class LocationAndPositionsController : Controller
    {
        [HttpGet, Route("location-and-positions")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("location-and-positions")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "RoleDescription");
        }
    }
}