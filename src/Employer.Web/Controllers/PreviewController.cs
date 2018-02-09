using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.Preview;

namespace Employer.Web.Controllers
{
    public class PreviewController : Controller
    {
        [HttpGet, Route("vacancy-preview")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("vacancy-preview")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "Submitted");
        }
    }
}