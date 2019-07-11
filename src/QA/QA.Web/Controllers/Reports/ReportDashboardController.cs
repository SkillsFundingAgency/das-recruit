using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers.Reports
{
    public class ReportDashboardController : Controller
    {
        private readonly ReportDashboardOrchestrator _orchestrator;

        public ReportDashboardController(ReportDashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet(RoutePaths.ReportsDashboardRoutePath, Name = RouteNames.ReportDashboard_Get)]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _orchestrator.GetDashboardViewModel();
            return View(vm);
        }

        [HttpGet(RoutePaths.ReportDownloadCsvRoutePath, Name = RouteNames.ReportDashboard_DownloadCsv)]
        public async Task<IActionResult> DownloadCsv([FromRoute] Guid reportId)
        {
            var vm = await _orchestrator.GetDownloadCsvAsync(reportId);
            return File(vm.Content, "application/octet-stream", $"{ToValidFileName(vm.ReportName)}.csv");
        }

        private string ToValidFileName(string text)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                text = text.Replace(c, '-');
            }

            return text;
        }
    }
}