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
            var dashboardStatsTask = await trainingProviderService.GetProviderDashboardStats(ukprn);

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

        public async Task<ProviderDashboardSummary> GetDashboardSummary(long ukprn)
        {
            var transferredVacanciesTask = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);
            var dashboardStatsTask = trainingProviderService.GetProviderDashboardStats(ukprn);

            await Task.WhenAll(transferredVacanciesTask, dashboardStatsTask);

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
                TransferredVacancies = transferredVacancies
            };
        }

        public async Task<ProviderDashboard> GetDashboardAsync(long ukprn,int page, FilteringOptions? status = null, string searchTerm = null)
        {
            var vacancySummariesTasks = vacancySummariesQuery.GetProviderOwnedVacancySummariesByUkprnAsync(ukprn, page, status, searchTerm);
            var transferredVacanciesTasks = vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn);

            await Task.WhenAll(vacancySummariesTasks, transferredVacanciesTasks);

            var vacancySummaries = vacancySummariesTasks.Result.Item1
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
                LastUpdated = timeProvider.Now,
                TotalVacancies = vacancySummariesTasks.Result.totalCount
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

        public Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync(int ukprn)
        {
            return apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync(ukprn: ukprn);
        }
    }
}