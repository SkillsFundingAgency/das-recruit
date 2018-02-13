using Esfa.Recruit.Employer.Web;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Sections;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Employer.Web.Controllers
{
    public class SectionsController : Controller
    {
        [HttpGet, Route("sections", Name = RouteNames.Sections_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };

            return View(vm);
        }
    }
}