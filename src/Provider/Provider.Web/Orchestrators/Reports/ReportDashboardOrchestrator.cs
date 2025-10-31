using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator(
        ILogger<ReportDashboardOrchestrator> logger,
        IProviderVacancyClient vacancyClient,
        IFeature feature)
        : ReportOrchestratorBase(logger, vacancyClient)
    {
        private readonly IProviderVacancyClient _vacancyClient = vacancyClient;
        private readonly bool _isReportsMigrationFeatureFlagEnabled = true;// feature.IsFeatureEnabled(FeatureNames.ReportsMigration);

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

        public async Task<ReportDownloadViewModel> GetDownloadCsvAsync(long ukprn, Guid reportId)
        {
            var report = await GetReportAsync(ukprn, reportId);

            var stream = new MemoryStream();

            if (_isReportsMigrationFeatureFlagEnabled)
            {
                await _vacancyClient.WriteApplicationSummaryReportsToCsv(stream, reportId);
            }
            else
            {
                var writeReportTask = _vacancyClient.WriteReportAsCsv(stream, report);
                var incrementReportDownloadCountTask = _vacancyClient.IncrementReportDownloadCountAsync(report.Id);

                await Task.WhenAll(writeReportTask, incrementReportDownloadCountTask);
            }
            return new ReportDownloadViewModel {
                Content = stream,
                ReportName = report.ReportName
            };
        }
    }
}