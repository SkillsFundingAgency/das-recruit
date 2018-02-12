using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.Submitted;

namespace Employer.Web.Controllers
{
    public class SubmittedController : Controller
    {
        [HttpGet, Route("vacancy-submitted")]
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