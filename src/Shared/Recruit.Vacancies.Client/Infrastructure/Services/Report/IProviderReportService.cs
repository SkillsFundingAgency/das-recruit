using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Report;
public interface IProviderReportService
{
    Task<GetProviderReportsApiResponse> GetReportsForProviderAsync(long ukprn);
    Task CreateProviderApplicationsReportAsync(Guid reportId, long ukprn, DateTime fromDate, DateTime toDate, Domain.Entities.VacancyUser user, string reportName);
    Task<GetReportApiResponse> GetReportAsync(Guid reportId);
    Task<GetReportDataApiResponse> GetReportDataAsync(Guid reportId);
}