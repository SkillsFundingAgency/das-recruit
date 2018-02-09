using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.EmployerDetails;

namespace Employer.Web.Controllers
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