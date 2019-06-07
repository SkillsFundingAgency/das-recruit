using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.EmployerAccountRoutePath)]
    public class EmployerVacancyController : Controller
    {        
        [HttpGet("create-vacancy", Name = RouteNames.EmployerCreateVacancy_Get)]        
        public IActionResult CreateVacancy()
        {
            TempData.Remove(TempDataKeys.EmployerVacancyMessage);
            return RedirectToRoute(RouteNames.CreateVacancy_Get);
        }

        [HttpGet("manage-vacancy", Name = RouteNames.EmployerManageVacancy_Get)]
        public IActionResult ManageVacancy()
        {
            TempData.Add(TempDataKeys.EmployerVacancyMessage, "fromMAHome");
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }        
    }
}