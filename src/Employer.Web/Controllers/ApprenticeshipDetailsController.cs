using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApprenticeshipDetails;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class ApprenticeshipDetailsController : Controller
    {
        [HttpGet("apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost("apprenticeship-details", Name = RouteNames.ApprenticeshipDetails_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.TrainingProvider_Index_Get);
        }
    }
}