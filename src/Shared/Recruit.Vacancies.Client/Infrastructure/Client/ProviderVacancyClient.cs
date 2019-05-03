using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient : IProviderVacancyClient
    {              
        public async Task<Guid> CreateVacancyAsync(string employerAccountId,
            long ukprn, string title, int numberOfPositions, VacancyUser user)
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
                numberOfPositions
            );

            await _messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public Task<ProviderDashboard> GetVacanciesAsync(long ukprn)
        {
            return _reader.GetProviderDashboardAsync(ukprn);
        }

        public Task GenerateVacancies(long ukprn)
        {
            return _providerDashboardService.ReBuildDashboardAsync(ukprn);
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

        public Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user)
        {
            var command = new SubmitVacancyCommand(vacancyId, user);

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
    }
}