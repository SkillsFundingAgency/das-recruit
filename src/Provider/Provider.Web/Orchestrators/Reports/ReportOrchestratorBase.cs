using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public abstract class ReportOrchestratorBase(ILogger logger, IProviderVacancyClient client)
    {
        protected async Task<Report> GetReportAsync(long ukprn, Guid reportId, ReportVersion version = ReportVersion.V2)
        {
            var report = await client.GetReportAsync(reportId, version);

            if (report == null)
            {
                logger.LogInformation("Cannot find report: {reportId}", reportId);
                throw new ReportNotFoundException($"Cannot find report: {reportId}");
            }

            if (report.Owner.OwnerType == ReportOwnerType.Provider && report.Owner.Ukprn == ukprn)
            {
                return report;
            }

            logger.LogWarning("Ukprn: {ukprn} does not have access to report: {reportId}", ukprn, reportId);
            throw new AuthorisationException($"Ukprn: {ukprn} does not have access to report: {reportId}");
        }
    }
}
