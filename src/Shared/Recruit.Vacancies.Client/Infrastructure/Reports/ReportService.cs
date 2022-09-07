using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly Func<ReportType, IReportStrategy> _reportStrategyAccessor;
        private readonly ITimeProvider _timeProvider;
        private readonly ICsvBuilder _csvBuilder;

        public ReportService(ILogger<ReportService> logger, 
            IReportRepository reportRepository,
            Func<ReportType, IReportStrategy> reportStrategyAccessor,
            ITimeProvider timeProvider,
            ICsvBuilder csvBuilder)
        {
            _logger = logger;
            _reportRepository = reportRepository;
            _reportStrategyAccessor = reportStrategyAccessor;
            _timeProvider = timeProvider;
            _csvBuilder = csvBuilder;
        }

        public async Task GenerateReportAsync(Guid reportId)
        {
            var report = await _reportRepository.GetReportAsync(reportId);

            if (report == null)
            {
                _logger.LogInformation("Report: {reportId} not found. Ignoring.", reportId);
                return;
            }

            if (report.Status == ReportStatus.Generated)
            {
                _logger.LogInformation("Report: {reportId} already generated. Ignoring.", reportId);
                return;
            }

            try
            {
                _logger.LogInformation("Report: {reportId} processing.", reportId);
                report.Status = ReportStatus.InProgress;
                report.GenerationStartedOn = _timeProvider.Now;

                await _reportRepository.UpdateAsync(report);
                _logger.LogInformation("Report: {reportId} in progress.", reportId);

                var reportStrategy = _reportStrategyAccessor(report.ReportType);
                var reportStrategyResult = await reportStrategy.GetReportDataAsync(report.Parameters);

                report = await _reportRepository.GetReportAsync(reportId);
                if (report.Status == ReportStatus.Generated)
                {
                    _logger.LogInformation("Report: {reportId} already generated in the meantime. Ignoring.", reportId);
                    return;
                }
                report.Headers = reportStrategyResult.Headers;
                report.Data = reportStrategyResult.Data;
                report.Status = ReportStatus.Generated;
                report.GeneratedOn = _timeProvider.Now;

                await _reportRepository.UpdateAsync(report);
                _logger.LogInformation("Report: {reportId} generated.", reportId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate report: {reportId}", report.Id);
                report.Status = ReportStatus.Failed;
                await _reportRepository.UpdateAsync(report);
                throw;
            }
        }

        public void WriteReportAsCsv(Stream stream, Report report)
        {
            if (report.Status != ReportStatus.Generated)
            {
                throw new Exception($"Cannot download report: {report.Id} as incorrect status: {report.Status}");
            }

            var reportStrategy = _reportStrategyAccessor(report.ReportType);

            var results = JArray.Parse(report.Data);

            _csvBuilder.WriteCsvToStream(stream, results, report.Headers, reportStrategy.ResolveFormat);
        }
    }
}
