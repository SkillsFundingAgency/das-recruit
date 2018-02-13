using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.LocationAndPositions;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class LocationAndPositionsController : Controller
    {
        [HttpGet, Route("location-and-positions", Name = RouteNames.LocationAndPosition_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("location-and-positions", Name = RouteNames.LocationAndPosition_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "RoleDescription");
        }
    }
}