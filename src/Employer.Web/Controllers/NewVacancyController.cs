using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.NewVacancy;

namespace Employer.Web.Controllers
{
    public class NewVacancyController : Controller
    {
        [HttpGet, Route("/new-vacancy")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel();
            return View(vm);
        }


        [HttpPost, Route("/new-vacancy")]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IndexViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }

            //dummy code
            Dummy.VacancyTitle = vm.Title;

            return RedirectToAction("Index", "Sections");
        }
        
    }
}