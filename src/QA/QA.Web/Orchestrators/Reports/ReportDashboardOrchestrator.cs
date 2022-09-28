using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator : ReportOrchestratorBase
    {
        private readonly IQaVacancyClient _vacancyClient;

        public ReportDashboardOrchestrator(ILogger<ReportDashboardOrchestrator> logger, IQaVacancyClient vacancyClient)
        : base(logger, vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<ReportsDashboardViewModel> GetDashboardViewModel()
        {
            var reports = await _vacancyClient.GetReportsAsync();

            var vm = new ReportsDashboardViewModel
            {
                ProcessingCount = reports.Count(r => r.IsProcessing),
                Reports = reports
                    .OrderByDescending(r => r.RequestedOn)
                    .Select(r => new ReportRowViewModel
                    {
                        ReportId = r.Id,
                        ReportName = r.ReportName,
                        DownloadCount = r.DownloadCount,
                        CreatedDate = r.RequestedOn.ToUkTime().AsGdsDateTime(),
                        CreatedBy = r.RequestedBy.Name,
                        Status = r.Status,
                        IsProcessing = r.IsProcessing
                    })
            };

            return vm;
        }

        public async Task<ReportDownloadViewModel> GetDownloadCsvAsync(Guid reportId)
        {
            var report = await GetReportAsync(reportId);

            var stream = new MemoryStream();
            
            var reportTask = _vacancyClient.WriteReportAsCsv(stream, report);

            var downloadIncrementTask = _vacancyClient.IncrementReportDownloadCountAsync(report.Id);

            await Task.WhenAll(reportTask, downloadIncrementTask);

            return new ReportDownloadViewModel
            {
                Content = stream,
                ReportName = report.ReportName
            };
        }
    }
}
