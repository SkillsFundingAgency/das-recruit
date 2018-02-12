using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class PreviewController : Controller
    {
        [HttpGet, Route("vacancy-preview")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("vacancy-preview")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "Submitted");
        }
    }
}