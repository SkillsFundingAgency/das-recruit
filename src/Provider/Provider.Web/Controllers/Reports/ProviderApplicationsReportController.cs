using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    [Route(RoutePaths.ProviderApplicationsReportRoutePath)]
    public class ProviderApplicationsReportController : Controller
    {
        private readonly ProviderApplicationsReportOrchestrator _orchestrator;

        public ProviderApplicationsReportController(ProviderApplicationsReportOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("create", Name = RouteNames.ProviderApplicationsReportCreate_Get)]
        public IActionResult Create()
        {
            var vm = _orchestrator.GetCreateViewModel();

            return View(vm);
        }

        [HttpPost("create", Name = RouteNames.ProviderApplicationsReportCreate_Post)]
        public async Task<IActionResult> Create(ProviderApplicationsReportCreateEditModel m)
        {
            if (ModelState.IsValid == false)
            {
                var vm = _orchestrator.GetCreateViewModel(m);

                return View(vm);
            }

            var reportId = await _orchestrator.PostCreateViewModelAsync(m, User.ToVacancyUser());

            return RedirectToRoute(RouteNames.ReportConfirmation_Get, new ReportRouteModel{ReportId = reportId, Ukprn = m.Ukprn});
        }
    }
}
