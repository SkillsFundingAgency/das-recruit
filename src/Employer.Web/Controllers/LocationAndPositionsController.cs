using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.LocationAndPositions;

namespace Employer.Web.Controllers
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