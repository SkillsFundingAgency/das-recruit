using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.TrainingProvider;

namespace Employer.Web.Controllers
{
    public class TrainingProviderController : Controller
    {
        [HttpGet, Route("training-provider")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("training-provider")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Confirm");
        }

        [HttpGet, Route("training-provider-confirm")]
        public IActionResult Confirm()
        {
            var vm = new ConfirmViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("training-provider-confirm")]
        public IActionResult Confirm(ConfirmViewModel vm)
        {
            return RedirectToAction("Index", "WageAndHours");
        }
    }
}