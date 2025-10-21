using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IProviderVacancyClient
    {
        Task<Guid> CreateVacancyAsync(string employerAccountId, long ukprn, string title, VacancyUser user, string accountLegalEntityPublicHashedId, string legalEntityName);
        Task<ProviderDashboard> GetDashboardAsync(long ukprn, string userId, int page, int pageSize, string sortColumn, string sortOrder, FilteringOptions? status = null, string searchTerm = null);
        Task SetupProviderAsync(long ukprn);
        Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn);
        Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
        Task<IEnumerable<EmployerInfo>> GetProviderEmployerVacancyDatasAsync(long ukprn, IList<string> employerAccountIds);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<Guid> CreateProviderApplicationsReportAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyUser user, string reportName);
        Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn);
        Task<Report> GetReportAsync(Guid reportId);
        Task WriteReportAsCsv(Stream stream, Report report);
        Task IncrementReportDownloadCountAsync(Guid reportId);
        Task CreateProviderApiVacancy(Guid id, string title, string employerAccountId, VacancyUser user);
        Task<ProviderDashboardSummary> GetDashboardSummary(long ukprn, string userId);
        Task<long> GetVacancyCount(long ukprn, FilteringOptions? filteringOptions, string searchTerm);
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync(int ukprn);
    }
}