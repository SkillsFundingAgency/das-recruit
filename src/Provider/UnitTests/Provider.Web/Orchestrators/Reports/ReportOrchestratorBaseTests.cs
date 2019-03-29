using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Reports
{
    public class ReportOrchestratorBaseTests
    {
        private readonly Guid _reportId = Guid.NewGuid();
        private const long _ukprn = 11111111;

        [Fact]
        public async Task GetReportAsync_ShouldThrowReportNotFoundExceptionWhenReportIsNotFound()
        {
            var orch = GetOrchestrator();

            var incorrectReportId = Guid.NewGuid();
            Func<Task<Report>> act = async () => await orch.GetTestReportAsync(0, incorrectReportId);

            var err = await act.Should().ThrowAsync<ReportNotFoundException>();

            err.WithMessage($"Cannot find report: {incorrectReportId}");
        }

        [Fact]
        public async Task GetReportAsync_ShouldThrowAuthorisationExceptionIfNotOwner()
        {
            var orch = GetOrchestrator();

            var incorrectUkprn = 22222222;
            Func<Task<Report>> act = async () => await orch.GetTestReportAsync(incorrectUkprn, _reportId);

            var err = await act.Should().ThrowAsync<AuthorisationException>();
            
            err.WithMessage($"Ukprn: {incorrectUkprn} does not have access to report: {_reportId}");
        }

        [Fact]
        public async Task GetReportAsync_ShouldReturnOwnedReport()
        {
            var orch = GetOrchestrator();

            var report = await orch.GetTestReportAsync(_ukprn, _reportId);

            report.Should().NotBeNull();
        }

        private TestReportOrchestrator GetOrchestrator()
        {
            var logger = new Mock<ILogger>();

            var reportOwner = new ReportOwner {
                OwnerType = ReportOwnerType.Provider,
                Ukprn = _ukprn
            };

            var report = new Report(_reportId, reportOwner, ReportStatus.New, "report name",
                ReportType.ProviderApplications, null, null, DateTime.Now);

            var repo = new Mock<IProviderVacancyClient>();
            repo.Setup(r => r.GetReportAsync(_reportId)).ReturnsAsync(report);

            return new TestReportOrchestrator(logger.Object, repo.Object);
        }

        private class TestReportOrchestrator : ReportOrchestratorBase
        {
            public TestReportOrchestrator(ILogger logger, IProviderVacancyClient client) : base(logger, client)
            {    
            }

            public Task<Report> GetTestReportAsync(long ukprn, Guid reportId)
            {
                return GetReportAsync(ukprn, reportId);
            }
        }
    }
}
