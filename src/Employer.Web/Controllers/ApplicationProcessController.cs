using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.ApplicationProcess;

namespace Employer.Web.Controllers
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