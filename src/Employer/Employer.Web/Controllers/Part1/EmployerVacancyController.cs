using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.EmployerAccountRoutePath)]
    public class EmployerVacancyController : Controller
    {        
        [HttpGet("create-vacancy", Name = RouteNames.EmployerCreateVacancy_Get)]
        public IActionResult CreateVacancy([FromRoute]string employerAccountId)
        {            
            return RedirectToRoute(RouteNames.CreateVacancy_Get, new { fromEmployer = true});
        }

        [HttpGet("manage-vacancy", Name = RouteNames.EmployerManageVacancy_Get)]
        public IActionResult ManageVacancy([FromRoute]string employerAccountId)
        {
            return RedirectToRoute(RouteNames.Dashboard_Account_Home, new { fromEmployer = true });
        }        
    }
}