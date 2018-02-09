using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class TrainingProviderController : Controller
    {
        [HttpGet, Route("accounts/{employerAccountId}/training-provider", Name = RouteNames.TrainingProvider_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("accounts/{employerAccountId}/training-provider", Name = RouteNames.TrainingProvider_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Confirm");
        }

        [HttpGet, Route("accounts/{employerAccountId}/training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public IActionResult Confirm()
        {
            var vm = new ConfirmViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("accounts/{employerAccountId}/training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public IActionResult Confirm(ConfirmViewModel vm)
        {
            return RedirectToAction("Index", "WageAndHours");
        }
    }
}