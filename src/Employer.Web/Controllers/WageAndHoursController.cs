using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.WageAndHours;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class WageAndHoursController : Controller
    {
        [HttpGet, Route("accounts/{employerAccountId}/wage-and-hours", Name = RouteNames.WageAndhours_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("accounts/{employerAccountId}/wage-and-hours", Name = RouteNames.WageAndhours_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "ApplicationProcess");
        }
    }
}