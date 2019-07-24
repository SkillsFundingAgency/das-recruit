using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class EmployerVacancyController : Controller
    {        
        [HttpGet("employer-create-vacancy", Name = RouteNames.EmployerCreateVacancy_Get)]        
        public IActionResult CreateVacancy(string ukprn, string programmeId)
        {
            if (string.IsNullOrWhiteSpace(ukprn) && string.IsNullOrWhiteSpace(programmeId))
            {
                TempData[TempDataKeys.ReferredFromMAHome] = true;
                return RedirectToRoute(RouteNames.CreateVacancyOptions_Get);
            }
            TempData[TempDataKeys.ReferredFromMAHome_UKPRN] = ukprn;
            TempData[TempDataKeys.ReferredFromMAHome_ProgrammeId] = programmeId;
            return RedirectToRoute(RouteNames.CreateVacancy_Get);
        }

        [HttpGet("employer-manage-vacancy", Name = RouteNames.EmployerManageVacancy_Get)]
        public IActionResult ManageVacancy()
        {
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }        
    }
}