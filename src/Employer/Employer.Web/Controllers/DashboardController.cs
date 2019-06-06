using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DashboardController(DashboardOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard([FromRoute]string employerAccountId, [FromQuery] string filter, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(filter))
                filter = Request.Cookies.GetCookie(CookieNames.DashboardFilter);

            if (string.IsNullOrWhiteSpace(filter) == false)
                Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.DashboardFilter, filter);

            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId, filter, page);
            if (TempData.ContainsKey(TempDataKeys.EmployerVacancyMessage) && !vm.HasVacancies)
            {
                return RedirectToRoute(RouteNames.EmployerCreateVacancy_Get);
            }

            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.DashboardInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.DashboardInfoMessage].ToString();

            return View(vm);
        }
    }
}