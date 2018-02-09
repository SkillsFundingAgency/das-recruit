using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class ApprenticeshipDetailsController : Controller
    {
        [HttpGet, Route("accounts/{employerAccountId}/apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("accounts/{employerAccountId}/apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "TrainingProvider");
        }
    }
}