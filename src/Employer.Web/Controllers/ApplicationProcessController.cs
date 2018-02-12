using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationProcess;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class ApplicationProcessController : Controller
    {
        [HttpGet, Route("application-process")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("application-process")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "Sections");
        }
    }
}