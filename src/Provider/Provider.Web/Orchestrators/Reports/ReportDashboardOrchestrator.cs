using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public interface IReportDashboardOrchestrator
    {
        Task<ReportsDashboardViewModel> GetDashboardViewModel(long ukprn);
        Task<ReportDownloadViewModel> GetDownloadCsvAsync(long ukprn, Guid reportId, ReportVersion version = ReportVersion.V2);
    }

    public class ReportDashboardOrchestrator(
        ILogger<ReportDashboardOrchestrator> logger,
        IProviderVacancyClient vacancyClient,
        IConfiguration configuration)
        : ReportOrchestratorBase(logger, vacancyClient), IReportDashboardOrchestrator
    {
        private readonly IProviderVacancyClient _vacancyClient = vacancyClient;

        public async Task<ReportsDashboardViewModel> GetDashboardViewModel(long ukprn)
        {
            var reportV2MigrationDate = configuration.GetValue<DateTime>("ReportsV1CutOffDate");

            var reports = await _vacancyClient.GetReportsForProviderAsync(ukprn);

            var vm = new ReportsDashboardViewModel 
            {
                Ukprn = ukprn,
                ProcessingCount = reports.Count(r => r.IsProcessing),
                ReportV2MigrationDate = reportV2MigrationDate,
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
                    IsProcessing = r.IsProcessing,
                    IsPreV2Migration = r.RequestedOn < reportV2MigrationDate,
                })
            };

            return vm;
        }

        public async Task<ReportDownloadViewModel> GetDownloadCsvAsync(long ukprn, Guid reportId, ReportVersion version = ReportVersion.V2)
        {
            var report = await GetReportAsync(ukprn, reportId);

            var stream = new MemoryStream();

            await _vacancyClient.WriteApplicationSummaryReportsToCsv(stream, reportId, version);
            
            return new ReportDownloadViewModel {
                Content = stream,
                ReportName = report.ReportName
            };
        }
    }
}