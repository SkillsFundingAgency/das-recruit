using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Reports
{
    public class ReportOrchestratorBaseTests
    {
        private readonly Guid _reportId = Guid.NewGuid();
        private const long Ukprn = 11111111;

        [Theory]
        [InlineData(ReportVersion.V1)]
        [InlineData(ReportVersion.V2)]
        public async Task GetReportAsync_ShouldThrowReportNotFoundExceptionWhenReportIsNotFound(ReportVersion version)
        {
            var orchestrator = GetOrchestrator(version);

            var incorrectReportId = Guid.NewGuid();
            Func<Task<Report>> act = async () => await orchestrator.GetTestReportAsync(0, incorrectReportId, version);

            var err = await act.Should().ThrowAsync<ReportNotFoundException>();

            err.WithMessage($"Cannot find report: {incorrectReportId}");
        }

        [Theory]
        [InlineData(ReportVersion.V1)]
        [InlineData(ReportVersion.V2)]
        public async Task GetReportAsync_ShouldThrowAuthorisationExceptionIfNotOwner(ReportVersion version)
        {
            var orchestrator = GetOrchestrator(version);

            const int incorrectUkprn = 22222222;
            Func<Task<Report>> act = async () => await orchestrator.GetTestReportAsync(incorrectUkprn, _reportId, version);

            var err = await act.Should().ThrowAsync<AuthorisationException>();
            
            err.WithMessage($"Ukprn: {incorrectUkprn} does not have access to report: {_reportId}");
        }

        [Theory]
        [InlineData(ReportVersion.V1)]
        [InlineData(ReportVersion.V2)]
        public async Task GetReportAsync_ShouldReturnOwnedReport(ReportVersion version)
        {
            var orchestrator = GetOrchestrator(version);

            var report = await orchestrator.GetTestReportAsync(Ukprn, _reportId, version);

            report.Should().NotBeNull();
        }

        private TestReportOrchestrator GetOrchestrator(ReportVersion version)
        {
            var logger = new Mock<ILogger>();

            var reportOwner = new ReportOwner {
                OwnerType = ReportOwnerType.Provider,
                Ukprn = Ukprn
            };

            var report = new Report(_reportId, reportOwner, ReportStatus.New, "report name",
                ReportType.ProviderApplications, null, null, DateTime.Now);

            var repo = new Mock<IProviderVacancyClient>();
            repo.Setup(r => r.GetReportAsync(_reportId, version)).ReturnsAsync(report);

            return new TestReportOrchestrator(logger.Object, repo.Object);
        }

        private class TestReportOrchestrator(ILogger logger, IProviderVacancyClient client)
            : ReportOrchestratorBase(logger, client)
        {
            public Task<Report> GetTestReportAsync(long ukprn, Guid reportId, ReportVersion version) 
                => GetReportAsync(ukprn, reportId, version);
        }
    }
}
