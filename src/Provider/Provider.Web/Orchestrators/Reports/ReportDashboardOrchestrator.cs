using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator(
        ILogger<ReportDashboardOrchestrator> logger,
        IProviderVacancyClient vacancyClient)
        : ReportOrchestratorBase(logger, vacancyClient)
    {
        private readonly IProviderVacancyClient _vacancyClient = vacancyClient;

        public async Task<ReportsDashboardViewModel> GetDashboardViewModel(long ukprn)
        {
            var reports = await _vacancyClient.GetReportsForProviderAsync(ukprn);

            var vm = new ReportsDashboardViewModel 
            {
                Ukprn = ukprn,
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

        public async Task<ReportDownloadViewModel> GetDownloadCsvAsync(long ukprn, Guid reportId, ReportVersion version = ReportVersion.V2)
        {
            var report = await GetReportAsync(ukprn, reportId, version);

            var stream = new MemoryStream();

            switch (version)
            {
                case ReportVersion.V1:
                    var writeReportTask = _vacancyClient.WriteReportAsCsv(stream, report);
                    var incrementReportDownloadCountTask = _vacancyClient.IncrementReportDownloadCountAsync(report.Id);
                    await Task.WhenAll(writeReportTask, incrementReportDownloadCountTask);
                    break;
                case ReportVersion.V2:
                    await _vacancyClient.WriteApplicationSummaryReportsToCsv(stream, reportId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
            
            return new ReportDownloadViewModel {
                Content = stream,
                ReportName = report.ReportName
            };
        }
    }
}