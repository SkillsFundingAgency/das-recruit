using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator : ReportOrchestratorBase
    {
        private readonly IProviderVacancyClient _vacancyClient;
        private readonly ServiceParameters _serviceParameters;

        public ReportDashboardOrchestrator(ILogger<ReportDashboardOrchestrator> logger, IProviderVacancyClient vacancyClient, ServiceParameters serviceParameters)
        :base(logger, vacancyClient)
        {
            _vacancyClient = vacancyClient;
            _serviceParameters = serviceParameters;
        }

        public async Task<ReportsDashboardViewModel> GetDashboardViewModel(long ukprn)
        {
            var reports = await _vacancyClient.GetReportsForProviderAsync(ukprn, _serviceParameters.VacancyType.GetValueOrDefault());

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

            _vacancyClient.WriteReportAsCsv(stream, report);

            await _vacancyClient.IncrementReportDownloadCountAsync(report.Id);

            return new ReportDownloadViewModel {
                Content = stream,
                ReportName = report.ReportName
            };
        }
    }
}
