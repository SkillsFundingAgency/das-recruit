using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class NewVacancyController : Controller
    {
        [HttpGet, Route("/new-vacancy", Name = RouteNames.NewVacancy_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel();
            return View(vm);
        }


        [HttpPost, Route("/new-vacancy", Name = RouteNames.NewVacancy_Index_Post)]
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