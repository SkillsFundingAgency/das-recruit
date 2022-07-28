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
            long ukprn, string title, VacancyUser user)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateProviderOwnedVacancyCommand(
                vacancyId,
                SourceOrigin.ProviderWeb,
                ukprn,
                employerAccountId,
                user,
                UserType.Provider,
                title
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
                title
            );
            
            await _messaging.SendCommandAsync(command);
            
            await AssignVacancyNumber(id);
        }
        
        public async Task<ProviderDashboard> GetDashboardAsync(long ukprn, VacancyType vacancyType)
        {
            var vacancySummariesTasks = _vacancySummariesQuery.GetProviderOwnedVacancySummariesByUkprnAsync(ukprn);
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

        public Task SetupProviderAsync(long ukprn)
        {
            var command = new SetupProviderCommand(ukprn);

            return _messaging.SendCommandAsync(command);
        }

        public async Task<Guid> CreateProviderApplicationsReportAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyUser user, string reportName)
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
                    { ReportParameterName.ToDate, toDate}
                },
                user,
                reportName)
            );

            return reportId;
        }

        public Task<List<ReportSummary>> GetReportsForProviderAsync(long ukprn)
        {
            return _reportRepository.GetReportsForProviderAsync<ReportSummary>(ukprn);
        }

        public Task<Report> GetReportAsync(Guid reportId)
        {
            return _reportRepository.GetReportAsync(reportId);
        }

        public void WriteReportAsCsv(Stream stream, Report report)
        {
            _reportService.WriteReportAsCsv(stream, report);
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