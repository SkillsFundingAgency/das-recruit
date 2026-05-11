using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    [Route(RoutePaths.ProviderApplicationsReportRoutePath)]
    public class ProviderApplicationsReportController(IProviderApplicationsReportOrchestrator orchestrator) : Controller
    {
        [HttpGet("create", Name = RouteNames.ProviderApplicationsReportCreate_Get)]
        public IActionResult Create([FromRoute]long ukprn)
        {
            var vm = orchestrator.GetCreateViewModel();
            vm.Ukprn = ukprn;
            return View(vm);
        }

        [HttpPost("create", Name = RouteNames.ProviderApplicationsReportCreate_Post)]
        public async Task<IActionResult> Create(ProviderApplicationsReportCreateEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = orchestrator.GetCreateViewModel(m);
                return View(vm);
            }

            var reportId = await orchestrator.PostCreateViewModelAsync(m, User.ToVacancyUser());
            TempData["NewReportId"] = reportId.ToString();

            return RedirectToRoute(RouteNames.ReportDashboard_Get, new {Ukprn = m.Ukprn});
        }
    }
}
