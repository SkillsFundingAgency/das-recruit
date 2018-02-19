using Esfa.Recruit.Employer.Web;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Sections;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using System;

namespace Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancy/{vacancyId}")]
    public class SectionsController : Controller
    {
        [HttpGet("sections", Name = RouteNames.Sections_Index_Get)]
        public IActionResult Index(Guid vacancyId)
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };

            return View(vm);
        }
    }
}