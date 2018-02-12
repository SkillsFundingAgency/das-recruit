using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.RoleDescription;

namespace Employer.Web.Controllers
{
    public class RoleDescriptionController : Controller
    {
        [HttpGet, Route("/role-description")]
        public IActionResult Index()
        {
            var vm = GetViewModel();
            return View(vm);
        }

        [HttpPost, Route("/role-description")]
        public IActionResult Index(IndexEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = GetViewModel(m);
                return View(vm);
            }

            //dummy code
            Dummy.VacancyTitle = m.Title;

            return RedirectToAction("Index", "CandidateProfile");
        }

        private IndexViewModel GetViewModel(IndexEditModel overrides = null)
        {
            //populate view model's reference data, dropdown lists, etc
            var vm = new IndexViewModel
            {
                CurrentVacancyTitle = Dummy.VacancyTitle,
                Title = Dummy.VacancyTitle
            };

            //update vm with posted data if applicable
            if(overrides != null)
            {
                vm.Title = overrides.Title;
            }

            return vm;
        }

    }
}