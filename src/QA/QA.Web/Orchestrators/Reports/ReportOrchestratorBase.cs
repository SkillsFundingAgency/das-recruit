using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web.Orchestrators.Reports
{
    public abstract class ReportOrchestratorBase
    {
        private readonly IQaVacancyClient _client;
        private readonly ILogger _logger;

        protected ReportOrchestratorBase(ILogger logger, IQaVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected async Task<Report> GetReportAsync(Guid reportId)
        {
            var report = await _client.GetReportAsync(reportId);

            if (report == null)
            {
                _logger.LogInformation("Cannot find report: {reportId}", reportId);
                throw new ReportNotFoundException($"Cannot find report: {reportId}");
            }

            if (report.Owner.OwnerType != ReportOwnerType.Qa)
                throw new AuthorisationException($"Unauthorised access to report: {reportId}");

            return report;
        }
    }
}
