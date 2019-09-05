using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Qa.Web.RouteModel;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Qa.Web.ViewModels.Reports.ApplicationsReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers.Reports
{
    [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
    [Route(RoutePaths.ApplicationsReportRoutePath)]
    public class ApplicationsReportController : Controller
    {
        private readonly ApplicationsReportOrchestrator _orchestrator;

        public ApplicationsReportController(ApplicationsReportOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("create", Name = RouteNames.ApplicationsReportCreate_Get)]
        public IActionResult Create()
        {
            var vm = _orchestrator.GetCreateViewModel();

            return View(vm);
        }

        [HttpPost("create", Name = RouteNames.ApplicationsReportCreate_Post)]
        public async Task<IActionResult> Create(ApplicationsReportCreateEditModel m)
        {
            if (ModelState.IsValid == false)
            {
                var vm = _orchestrator.GetCreateViewModel(m);

                return View(vm);
            }

            var reportId = await _orchestrator.PostCreateViewModelAsync(m, User.GetVacancyUser());

            return RedirectToRoute(RouteNames.ReportConfirmation_Get, new ReportRouteModel { ReportId = reportId });
        }
    }
}
