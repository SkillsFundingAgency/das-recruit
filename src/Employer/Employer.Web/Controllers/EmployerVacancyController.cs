using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class EmployerVacancyController : Controller
    {        
        [HttpGet("employer-create-vacancy", Name = RouteNames.EmployerCreateVacancy_Get)]        
        public IActionResult CreateVacancy()
        {
            TempData[TempDataKeys.ReferredFromMAHome] = true;
            return RedirectToRoute(RouteNames.CreateVacancyOptions_Get);
        }

        [HttpGet("employer-manage-vacancy", Name = RouteNames.EmployerManageVacancy_Get)]
        public IActionResult ManageVacancy()
        {
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }        
    }
}