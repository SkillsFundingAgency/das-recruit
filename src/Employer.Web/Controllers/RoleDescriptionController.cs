using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class RoleDescriptionController : Controller
    {
        [HttpGet("role-description", Name = RouteNames.RoleDescription_Index_Get)]
        public IActionResult Index()
        {
            var vm = GetViewModel();
            return View(vm);
        }

        [HttpPost("role-description", Name =  RouteNames.RoleDescription_Index_Post)]
        public IActionResult Index(IndexEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = GetViewModel(m);
                return View(vm);
            }

            //dummy code
            Dummy.VacancyTitle = m.Title;

            return RedirectToRoute(RouteNames.CandidateProfile_Index_Get);
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