using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public abstract class ReportOrchestratorBase
    {
        private readonly IProviderVacancyClient _client;
        private readonly ILogger _logger;

        protected ReportOrchestratorBase(ILogger logger, IProviderVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected async Task<Report> GetReportAsync(long ukprn, Guid reportId)
        {
            var report = await _client.GetReportAsync(reportId);

            if (report == null)
            {
                _logger.LogInformation("Cannot find report: {reportId}", reportId);
                throw new ReportNotFoundException($"Cannot find report: {reportId}");
            }

            if (report.Owner.OwnerType == ReportOwnerType.Provider &&
                report.Owner.Ukprn == ukprn)
            {
                return report;
            }

            _logger.LogWarning("Ukprn: {ukprn} does not have access to report: {reportId}", ukprn, reportId);
            throw new AuthorisationException($"Ukprn: {ukprn} does not have access to report: {reportId}");
        }
    }
}
