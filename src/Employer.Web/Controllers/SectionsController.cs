using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Sections;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Employer.Web.Controllers
{
    public class SectionsController : Controller
    {
        [HttpGet, Route("sections")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };

            return View(vm);
        }

        [HttpPost, Route("sections")]
        public IActionResult Index(IndexViewModel vm)
        {
            return View(vm);
        }
    }
}