using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    public class ReportDashboardController(IReportDashboardOrchestrator orchestrator) : Controller
    {
        [HttpGet(RoutePaths.ReportsDashboardRoutePath, Name = RouteNames.ReportDashboard_Get)]
        public async Task<IActionResult> Dashboard([FromRoute] long ukprn)
        {
            var vm = await orchestrator.GetDashboardViewModel(ukprn);
            if (TempData.TryGetValue("NewReportId", out var newReportIdObj) &&
                Guid.TryParse(newReportIdObj?.ToString(), out var newReportId))
            {
                var newReport = vm.Reports?.FirstOrDefault(r => r.ReportId == newReportId);
                if (newReport != null)
                {
                    vm.ShowSuccessBanner = true;
                    vm.SuccessReportName = newReport.ReportName;
                    TempData.Remove("NewReportId");
                }
            }
            return View(vm);
        }

        [HttpGet(RoutePaths.ReportDownloadCsvRoutePath, Name = RouteNames.ReportDashboard_DownloadCsv)]
        public async Task<IActionResult> DownloadCsv([FromRoute] long ukprn, [FromRoute] Guid reportId, [FromRoute] ReportVersion version = ReportVersion.V2)
        {
            var vm = await orchestrator.GetDownloadCsvAsync(ukprn, reportId, version);
            return File(vm.Content, "application/octet-stream", $"{ToValidFileName(vm.ReportName)}.csv");
        }

        [HttpGet]
        [Route(RoutePaths.ReportDataSpecificationsRoutePath, Name = RouteNames.ReportDataSpecifications_Get)]
        public IActionResult ReportDataSpecifications([FromRoute] long ukprn) => View();

        private static string ToValidFileName(string text) 
            => Path.GetInvalidFileNameChars().Aggregate(text, (current, c) => current.Replace(c, '-'));
    }
}