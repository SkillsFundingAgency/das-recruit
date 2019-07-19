using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Qa.Web.Orchestrators.Reports
{
    public class ReportOrchestratorBaseTests
    {
        private readonly Guid _reportId = Guid.NewGuid();
        private Report Report;

        [Fact]
        public async Task GetReportAsync_ShouldThrowReportNotFoundExceptionWhenReportIsNotFound()
        {
            var orch = GetOrchestrator();

            var incorrectReportId = Guid.NewGuid();
            Func<Task<Report>> act = async () => await orch.GetTestReportAsync(incorrectReportId);

            var err = await act.Should().ThrowAsync<ReportNotFoundException>();

            err.WithMessage($"Cannot find report: {incorrectReportId}");
        }

        [Fact]
        public async Task GetReportAsync_ShouldThrowAuthorisationExceptionIfNotOwner()
        {
            var orch = GetOrchestrator();

            Report.Owner = new ReportOwner
            {
                OwnerType = ReportOwnerType.Provider
            };
     
            Func<Task<Report>> act = async () => await orch.GetTestReportAsync(_reportId);

            var err = await act.Should().ThrowAsync<AuthorisationException>();

            err.WithMessage($"Unauthorised access to report: {_reportId}");
        }

        [Fact]
        public async Task GetReportAsync_ShouldReturnOwnedReport()
        {
            var orch = GetOrchestrator();

            Report = await orch.GetTestReportAsync(_reportId);

            Report.Should().NotBeNull();
        }

        private TestReportOrchestrator GetOrchestrator()
        {
            var logger = new Mock<ILogger>();

            var reportOwner = new ReportOwner
            {
                OwnerType = ReportOwnerType.Qa
            };

            Report = new Report(_reportId, reportOwner, ReportStatus.New, "report name",
                ReportType.QaApplications, null, null, DateTime.Now);

            var repo = new Mock<IQaVacancyClient>();
            repo.Setup(r => r.GetReportAsync(_reportId)).ReturnsAsync(Report);

            return new TestReportOrchestrator(logger.Object, repo.Object);
        }

        private class TestReportOrchestrator : ReportOrchestratorBase
        {
            public TestReportOrchestrator(ILogger logger, IQaVacancyClient client) : base(logger, client)
            {
            }

            public Task<Report> GetTestReportAsync(Guid reportId)
            {
                return GetReportAsync(reportId);
            }
        }
    }

}
