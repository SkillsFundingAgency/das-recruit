using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Microsoft.Extensions.Logging;

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
            return await vacancySummariesQuery.VacancyCount(ukprn, string.Empty, filteringOptions, searchTerm, OwnerType.Provider);
        }

        public async Task<ProviderDashboardSummary> GetDashboardSummary(long ukprn)
        {
            var dashboardTask = vacancySummariesQuery.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, IsMongoMigrationFeatureEnabled);
            var transferredVacanciesTask = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);
            var dashboardStatsTask = trainingProviderService.GetProviderDashboardStats(ukprn);

            await Task.WhenAll(dashboardTask, transferredVacanciesTask, dashboardStatsTask);

            var dashboardValue = dashboardTask.Result;
            var transferredVacancies = transferredVacanciesTask.Result.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                });
            var dashboardStats = dashboardStatsTask.Result;

            var dashboard = dashboardValue.VacancyStatusDashboard;
            var dashboardApplications = dashboardValue.VacancyApplicationsDashboard;

            return new ProviderDashboardSummary
            {
                Closed = dashboard.FirstOrDefault(c => c.Status == VacancyStatus.Closed)?.StatusCount ?? 0,
                Draft = dashboard.SingleOrDefault(c => c.Status == VacancyStatus.Draft)?.StatusCount ?? 0,
                Review = dashboard.SingleOrDefault(c => c.Status == VacancyStatus.Review)?.StatusCount ?? 0,
                Referred = (dashboard.SingleOrDefault(c => c.Status == VacancyStatus.Referred)?.StatusCount ?? 0) + (dashboard.SingleOrDefault(c => c.Status == VacancyStatus.Rejected)?.StatusCount ?? 0),
                Live = dashboard.Where(c => c.Status == VacancyStatus.Live).Sum(c => c.StatusCount),
                Submitted = dashboard.SingleOrDefault(c => c.Status == VacancyStatus.Submitted)?.StatusCount ?? 0,
                NumberOfNewApplications = IsMongoMigrationFeatureEnabled 
                    ? dashboardStats.NewApplicationsCount 
                    : dashboardApplications.Where(c => c.Status is VacancyStatus.Live or VacancyStatus.Closed).Sum(x => x.NoOfNewApplications),
                NumberOfEmployerReviewedApplications = IsMongoMigrationFeatureEnabled
                    ? dashboardStats.EmployerReviewedApplicationsCount
                    : dashboardApplications.Where(c => c.Status is VacancyStatus.Live or VacancyStatus.Closed).Sum(x => x.NumberOfEmployerReviewedApplications),
                NumberOfSuccessfulApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live && !c.ClosingSoon).Sum(x => x.NoOfSuccessfulApplications)
                                                 + dashboardApplications.Where(c => c.Status == VacancyStatus.Closed && !c.ClosingSoon).Sum(x => x.NoOfSuccessfulApplications),
                NumberOfUnsuccessfulApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live && !c.ClosingSoon).Sum(x => x.NoOfUnsuccessfulApplications)
                                                   + dashboardApplications.Where(c => c.Status == VacancyStatus.Closed && !c.ClosingSoon).Sum(x => x.NoOfUnsuccessfulApplications),
                NumberClosingSoon = dashboard.FirstOrDefault(c => c.Status == VacancyStatus.Live && c.ClosingSoon)?.StatusCount ?? 0,
                NumberClosingSoonWithNoApplications = dashboardValue.VacanciesClosingSoonWithNoApplications,
                TransferredVacancies = transferredVacancies
            };
        }

        public async Task<ProviderDashboard> GetDashboardAsync(long ukprn,int page, FilteringOptions? status = null, string searchTerm = null)
        {
            var vacancySummariesTasks = vacancySummariesQuery.GetProviderOwnedVacancySummariesByUkprnAsync(ukprn, page, status, searchTerm);
            var transferredVacanciesTasks = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);

            await Task.WhenAll(vacancySummariesTasks, transferredVacanciesTasks);

            var vacancySummaries = vacancySummariesTasks.Result
                .Where(c=> !c.IsTraineeship).ToList();
            var transferredVacancies = transferredVacanciesTasks.Result.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                });

            foreach (var summary in vacancySummaries)
            {
                await UpdateWithTrainingProgrammeInfo(summary);
            }

            return new ProviderDashboard
            {
                Id = QueryViewType.ProviderDashboard.GetIdValue(ukprn),
                Vacancies = vacancySummaries,
                TransferredVacancies = transferredVacancies,
                LastUpdated = timeProvider.Now
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

            await messaging.SendCommandAsync(new CreateReportCommand(
                reportId,
                owner,
                ReportType.ProviderApplications,
                new Dictionary<string, object> {
                    { ReportParameterName.Ukprn, ukprn},
                    { ReportParameterName.FromDate, fromDate},
                    { ReportParameterName.ToDate, toDate}
                },
                user,
                reportName)
            );

            return reportId;
        }

        public Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn)
        {
            return reportRepository.GetReportsForProviderAsync<ReportSummary>(ukprn);
        }

        public Task<Report> GetReportAsync(Guid reportId)
        {
            return reportRepository.GetReportAsync(reportId);
        }

        public async Task WriteReportAsCsv(Stream stream, Report report)
        {
            await reportService.WriteReportAsCsv(stream, report);
        }

        public Task IncrementReportDownloadCountAsync(Guid reportId)
        {
            return reportRepository.IncrementReportDownloadCountAsync(reportId);
        }

        private async Task UpdateWithTrainingProgrammeInfo(VacancySummary summary)
        {
            if (summary.ProgrammeId != null)
            {
                var programme = await apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(summary.ProgrammeId);

                if (programme == null)
                {
                    logger.LogWarning($"No training programme found for ProgrammeId: {summary.ProgrammeId}");
                }
                else
                {
                    summary.TrainingTitle = programme.Title;
                    summary.TrainingType = programme.ApprenticeshipType;
                    summary.TrainingLevel = programme.ApprenticeshipLevel;
                }
            }
        }

    }
}