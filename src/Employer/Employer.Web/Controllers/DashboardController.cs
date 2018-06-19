using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(IHostingEnvironment hostingEnvironment, DashboardOrchestrator orchestrator)
        {
            _hostingEnvironment = hostingEnvironment;
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard([FromRoute]string employerAccountId, [FromQuery]string statusFilter)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId);

            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
            {
                vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();
            }

            ManageDashboardFilter(statusFilter, vm);

            return View(vm);
        }

        private void ManageDashboardFilter(string statusFilter, DashboardViewModel vm)
        {
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<VacancyStatus>(statusFilter, out var filter))
            {
                vm.Filter = filter;
                Response.Cookies.Append(CookieNames.VacancyStatusFilter, filter.ToString(), EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            }
            else
            {
                Response.Cookies.Delete(CookieNames.VacancyStatusFilter, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            }
        }
    }
}