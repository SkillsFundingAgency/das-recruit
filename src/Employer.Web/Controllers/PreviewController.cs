using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Employer.Web.Configuration.Routes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class PreviewController : Controller
    {
        [HttpGet, Route("vacancy-preview", Name = RouteNames.Preview_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("vacancy-preview", Name = RouteNames.Preview_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "Submitted");
        }
    }
}