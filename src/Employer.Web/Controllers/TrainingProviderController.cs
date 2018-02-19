using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class TrainingProviderController : Controller
    {
        [HttpGet("training-provider", Name = RouteNames.TrainingProvider_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost("training-provider", Name = RouteNames.TrainingProvider_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.TrainingProvider_Confirm_Get);
        }

        [HttpGet("training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Get)]
        public IActionResult Confirm()
        {
            var vm = new ConfirmViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost("training-provider-confirm", Name = RouteNames.TrainingProvider_Confirm_Post)]
        public IActionResult Confirm(ConfirmViewModel vm)
        {
            return RedirectToRoute(RouteNames.WageAndhours_Index_Get);
        }
    }
}