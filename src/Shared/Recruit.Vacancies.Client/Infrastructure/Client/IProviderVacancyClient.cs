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
        Task<Guid> CreateVacancyAsync(string employerAccountId, long ukprn, string title, int numberOfPositions, VacancyUser user);
        Task GenerateDashboard(long ukprn);
        Task<ProviderDashboard> GetDashboardAsync(long ukprn);
        Task SetupProviderAsync(long ukprn);
        Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn);
        Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
        Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user);
        Task CloseVacancyAsync(Guid vacancyId, VacancyUser user);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<Guid> CreateProviderApplicationsReportAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyUser user, string reportName);
        Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn);
        Task<Report> GetReportAsync(Guid reportId);
        void WriteReportAsCsv(Stream stream, Report report);
        Task IncrementReportDownloadCountAsync(Guid reportId);
    }
}