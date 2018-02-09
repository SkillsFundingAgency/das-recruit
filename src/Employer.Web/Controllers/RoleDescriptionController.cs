using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class RoleDescriptionController : Controller
    {
        [HttpGet, Route("accounts/{employerAccountId}/role-description", Name = RouteNames.RoleDescription_Index_Get)]
        public IActionResult Index()
        {
            var vm = GetViewModel();
            return View(vm);
        }

        [HttpPost, Route("accounts/{employerAccountId}/role-description", Name =  RouteNames.RoleDescription_Index_Post)]
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