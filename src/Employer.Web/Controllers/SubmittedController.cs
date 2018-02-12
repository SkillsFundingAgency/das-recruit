using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class SubmittedController : Controller
    {
        [HttpGet, Route("vacancy-submitted", Name = RouteNames.Submitted_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle,
                VacancyReference = "12345678"
            };
            return View(vm);
        }
    }
}