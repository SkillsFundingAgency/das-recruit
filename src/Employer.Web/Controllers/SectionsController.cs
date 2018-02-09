using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.Sections;

namespace Employer.Web.Controllers
{
    public class SectionsController : Controller
    {
        [HttpGet, Route("sections")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };

            return View(vm);
        }

        [HttpPost, Route("sections")]
        public IActionResult Index(IndexViewModel vm)
        {
            return View(vm);
        }
    }
}