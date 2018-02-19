using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationProcess;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using System;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class ApplicationProcessController : Controller
    {
        [HttpGet("application-process", Name = RouteNames.ApplicationProcess_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost("application-process", Name = RouteNames.ApplicationProcess_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.Sections_Index_Get);
        }
    }
}