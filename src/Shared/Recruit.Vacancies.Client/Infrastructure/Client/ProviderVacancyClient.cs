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

            await _messaging.SendCommandAsync(command);

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

            await _messaging.SendCommandAsync(command);

            await AssignVacancyNumber(id);
        }

        public async Task<long> GetVacancyCount(long ukprn, VacancyType vacancyType, FilteringOptions? filteringOptions, string searchTerm)
        {
            return await _vacancySummariesQuery.VacancyCount(ukprn, string.Empty, vacancyType, filteringOptions, searchTerm, OwnerType.Provider);
        }

        public async Task<ProviderDashboardSummary> GetDashboardSummary(long ukprn, VacancyType vacancyType)
        {
            var dashboardTask = _vacancySummariesQuery.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, vacancyType);
            var transferredVacanciesTask = _vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn, vacancyType);

            await Task.WhenAll(dashboardTask, transferredVacanciesTask);

            var dashboardValue = dashboardTask.Result;
            var transferredVacancies = transferredVacanciesTask.Result.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                });

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
                NumberOfNewApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x => x.NoOfNewApplications),
                NumberOfEmployerReviewedApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x => x.NumberOfEmployerReviewedApplications),
                NumberOfSuccessfulApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live && !c.ClosingSoon).Sum(x => x.NoOfSuccessfulApplications)
                                                 + dashboardApplications.Where(c => c.Status == VacancyStatus.Closed && !c.ClosingSoon).Sum(x => x.NoOfSuccessfulApplications),
                NumberOfUnsuccessfulApplications = dashboardApplications.Where(c => c.Status == VacancyStatus.Live && !c.ClosingSoon).Sum(x => x.NoOfUnsuccessfulApplications)
                                                   + dashboardApplications.Where(c => c.Status == VacancyStatus.Closed && !c.ClosingSoon).Sum(x => x.NoOfUnsuccessfulApplications),
                NumberClosingSoon = dashboard.FirstOrDefault(c => c.Status == VacancyStatus.Live && c.ClosingSoon)?.StatusCount ?? 0,
                NumberClosingSoonWithNoApplications = dashboardValue.VacanciesClosingSoonWithNoApplications,
                TransferredVacancies = transferredVacancies
            };
        }

        public async Task<ProviderDashboard> GetDashboardAsync(long ukprn, VacancyType vacancyType,int page, FilteringOptions? status = null, string searchTerm = null)
        {
            var vacancySummariesTasks = _vacancySummariesQuery.GetProviderOwnedVacancySummariesByUkprnAsync(ukprn, vacancyType, page, status, searchTerm);
            var transferredVacanciesTasks = _vacancySummariesQuery.GetTransferredFromProviderAsync(ukprn, vacancyType);

            await Task.WhenAll(vacancySummariesTasks, transferredVacanciesTasks);

            var vacancySummaries = vacancySummariesTasks.Result
                .Where(c=>vacancyType == VacancyType.Traineeship ? c.IsTraineeship : !c.IsTraineeship).ToList();
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
                Id = vacancyType == VacancyType.Apprenticeship ? QueryViewType.ProviderDashboard.GetIdValue(ukprn) :  QueryViewType.ProviderTraineeshipDashboard.GetIdValue(ukprn),
                Vacancies = vacancySummaries,
                TransferredVacancies = transferredVacancies,
                LastUpdated = _timeProvider.Now
            };
        }

        public Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn)
        {
            return _reader.GetProviderVacancyDataAsync(ukprn);
        }

        public Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId)
        {
            return _reader.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);
        }

        public Task<IEnumerable<EmployerInfo>> GetProviderEmployerVacancyDatasAsync(long ukprn, IList<string> employerAccountIds)
        {
            return _reader.GetProviderEmployerVacancyDatasAsync(ukprn, employerAccountIds);
        }

        public Task SetupProviderAsync(long ukprn)
        {
            var command = new SetupProviderCommand(ukprn);

            return _messaging.SendCommandAsync(command);
        }

        public async Task<Guid> CreateProviderApplicationsReportAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyUser user, string reportName, VacancyType vacancyType)
        {
            var reportId = Guid.NewGuid();

            var owner = new ReportOwner
            {
                OwnerType = ReportOwnerType.Provider,
                Ukprn = ukprn
            };

            await _messaging.SendCommandAsync(new CreateReportCommand(
                reportId,
                owner,
                ReportType.ProviderApplications,
                new Dictionary<string, object> {
                    { ReportParameterName.Ukprn, ukprn},
                    { ReportParameterName.FromDate, fromDate},
                    { ReportParameterName.ToDate, toDate},
                    { ReportParameterName.VacancyType, vacancyType.ToString()}
                },
                user,
                reportName)
            );

            return reportId;
        }

        public Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn, VacancyType vacancyType)
        {
            return _reportRepository.GetReportsForProviderAsync<ReportSummary>(ukprn, vacancyType);
        }

        public Task<Report> GetReportAsync(Guid reportId)
        {
            return _reportRepository.GetReportAsync(reportId);
        }

        public async Task WriteReportAsCsv(Stream stream, Report report)
        {
            await _reportService.WriteReportAsCsv(stream, report);
        }

        public Task IncrementReportDownloadCountAsync(Guid reportId)
        {
            return _reportRepository.IncrementReportDownloadCountAsync(reportId);
        }

        private async Task UpdateWithTrainingProgrammeInfo(VacancySummary summary)
        {
            if (summary.ProgrammeId != null)
            {
                var programme = await _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(summary.ProgrammeId);

                if (programme == null)
                {
                    _logger.LogWarning($"No training programme found for ProgrammeId: {summary.ProgrammeId}");
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