using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Report = Esfa.Recruit.Vacancies.Client.Domain.Entities.Report;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient : IProviderVacancyClient
    {
        public async Task<Guid> CreateVacancyAsync(string employerAccountId,
            long ukprn, string title, VacancyUser user, string accountLegalEntityPublicHashedId, string legalEntityName)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateProviderOwnedVacancyCommand(
                vacancyId,
                SourceOrigin.ProviderWeb,
                ukprn,
                employerAccountId,
                user,
                UserType.Provider,
                title,
                accountLegalEntityPublicHashedId,
                legalEntityName
            );

            await messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public async Task CreateProviderApiVacancy(Guid id, string title, string employerAccountId, VacancyUser user)
        {
            var command = new CreateProviderOwnedVacancyCommand(
                id,
                SourceOrigin.Api,
                user.Ukprn.Value,
                employerAccountId,
                user,
                UserType.Provider,
                title,
                null,
                null
            );

            await messaging.SendCommandAsync(command);

            await AssignVacancyNumber(id);
        }

        public async Task<long> GetVacancyCount(long ukprn, FilteringOptions? filteringOptions, string searchTerm)
        {
            var dashboardStatsTask = await trainingProviderService.GetProviderDashboardStats(ukprn, "");

            switch (filteringOptions)
            {
                case FilteringOptions.NewApplications:
                    return dashboardStatsTask.NewApplicationsCount;
                case FilteringOptions.AllSharedApplications:
                    return dashboardStatsTask.AllSharedApplicationsCount;
                case FilteringOptions.EmployerReviewedApplications:
                    return dashboardStatsTask.EmployerReviewedApplicationsCount;
                default:
                    return await vacancySummariesQuery.VacancyCount(ukprn, string.Empty, filteringOptions, searchTerm, OwnerType.Provider);
            }
        }

        public async Task<ProviderDashboardSummary> GetDashboardSummary(long ukprn, string userId)
        {
            var transferredVacanciesTask = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);
            var dashboardStatsTask = trainingProviderService.GetProviderDashboardStats(ukprn, userId);
            var alertsTask = trainingProviderService.GetProviderAlerts(Convert.ToInt32(ukprn), userId);

            await Task.WhenAll(transferredVacanciesTask, alertsTask, dashboardStatsTask);
            var alerts = await alertsTask;

            var transferredVacancies = transferredVacanciesTask.Result.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                });
            var dashboardStats = dashboardStatsTask.Result;

            return new ProviderDashboardSummary
            {
                Closed = dashboardStats.ClosedVacanciesCount,
                Draft = dashboardStats.DraftVacanciesCount,
                Review = dashboardStats.ReviewVacanciesCount,
                Referred = dashboardStats.ReferredVacanciesCount,
                Live = dashboardStats.LiveVacanciesCount,
                Submitted = dashboardStats.SubmittedVacanciesCount,
                NumberOfNewApplications = dashboardStats.NewApplicationsCount,
                NumberOfEmployerReviewedApplications = dashboardStats.EmployerReviewedApplicationsCount,
                NumberOfSuccessfulApplications = dashboardStats.SuccessfulApplicationsCount,
                NumberOfUnsuccessfulApplications = dashboardStats.UnsuccessfulApplicationsCount,
                NumberClosingSoon = dashboardStats.ClosingSoonVacanciesCount,
                NumberClosingSoonWithNoApplications = dashboardStats.ClosingSoonWithNoApplications,
                ProviderTransferredVacanciesAlert = alerts.ProviderTransferredVacanciesAlert,
                WithdrawnVacanciesAlert = alerts.WithdrawnVacanciesAlert,
                TransferredVacancies = transferredVacancies
            };
        }

        public async Task<ProviderDashboard> GetDashboardAsync(long ukprn, string userId, int page, int pageSize, string sortColumn, string sortOrder, FilteringOptions? status = null, string searchTerm = null)
        {
            var vacancySummariesTasks =
                trainingProviderService.GetProviderVacancies(Convert.ToInt32(ukprn), page, pageSize, sortColumn, sortOrder, status ?? FilteringOptions.Dashboard, searchTerm);
            var alertsTask = trainingProviderService.GetProviderAlerts(Convert.ToInt32(ukprn), userId);
            var transferredVacanciesTasks = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);


            await Task.WhenAll(vacancySummariesTasks, alertsTask, transferredVacanciesTasks);

            var vacancySummariesResult = await vacancySummariesTasks;
            var alerts = await alertsTask;

            var vacancySummaries = vacancySummariesResult.VacancySummaries
                .Where(c=> !c.IsTraineeship).ToList();
            var transferredVacancies = transferredVacanciesTasks.Result.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                });

            return new ProviderDashboard
            {
                Id = QueryViewType.ProviderDashboard.GetIdValue(ukprn),
                Vacancies = vacancySummaries,
                TransferredVacancies = transferredVacancies,
                LastUpdated = timeProvider.Now,
                TotalVacancies = vacancySummariesResult.PageInfo.TotalCount,
                WithdrawnVacanciesAlert = alerts.WithdrawnVacanciesAlert,
                ProviderTransferredVacanciesAlert = alerts.ProviderTransferredVacanciesAlert,
            };
        }

        public Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn)
        {
            return reader.GetProviderVacancyDataAsync(ukprn);
        }

        public Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId)
        {
            return reader.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);
        }

        public Task<IEnumerable<EmployerInfo>> GetProviderEmployerVacancyDatasAsync(long ukprn, IList<string> employerAccountIds)
        {
            return reader.GetProviderEmployerVacancyDatasAsync(ukprn, employerAccountIds);
        }

        public Task SetupProviderAsync(long ukprn)
        {
            var command = new SetupProviderCommand(ukprn);

            return messaging.SendCommandAsync(command);
        }

        public async Task<Guid> CreateProviderApplicationsReportAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyUser user, string reportName)
        {
            var reportId = Guid.NewGuid();

            var owner = new ReportOwner
            {
                OwnerType = ReportOwnerType.Provider,
                Ukprn = ukprn
            };

            // Report generation is handled by the ProviderReportService which calls the Outer API. so that report is created in the SQL DB.
            await providerReportService.CreateProviderApplicationsReportAsync(reportId, ukprn, fromDate, toDate, user,
                reportName);
            
            return reportId;
        }

        public async Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn)
        {
            // Report retrieval is handled by the ProviderReportService which calls the Outer API
            var providerReports = await providerReportService.GetReportsForProviderAsync(ukprn);
            if (providerReports?.Reports == null || providerReports.Reports.Count == 0)
            {
                return [];
            }
            return providerReports.Reports.Select(Domain.Reports.Report.ToReportSummary).ToList();
        }

        public async Task<Report> GetReportAsync(Guid reportId)
        {
            // Report retrieval is handled by the ProviderReportService which calls the Outer API
            var response = await providerReportService.GetReportAsync(reportId);
            return response?.Report?.ToEntity(response.Report);
        }

        public async Task WriteApplicationSummaryReportsToCsv(Stream stream, Guid reportId, ReportVersion version = ReportVersion.V2)
        {
            var response = await providerReportService.GetReportDataAsync(reportId);

            switch (version)
            {
                case ReportVersion.V1:
                    await reportService.WriteApplicationSummaryReportsV1ToCsv(stream, response.Reports.Select(c => (ApplicationSummaryCsvReportV1)c).ToList());
                    break;
                case ReportVersion.V2:
                    await reportService.WriteApplicationSummaryReportsV2ToCsv(stream, response.Reports.Select(r => (ApplicationSummaryCsvReportV2)r).ToList());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync(int ukprn)
            => apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync(ukprn: ukprn);
    }
}