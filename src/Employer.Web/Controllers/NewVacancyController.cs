using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;

namespace Esfa.Recruit.Employer.Web.Controllers
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