using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Qa.Web.ViewModels.Reports;
using Esfa.Recruit.Qa.Web.ViewModels.Reports.ApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using Xunit;

namespace UnitTests.Qa.Web.Orchestrators.Reports
{
    public class ApplicationsReportOrchestratorTests
    {
        private readonly Mock<IQaVacancyClient> _client = new Mock<IQaVacancyClient>();

        [Theory]
        [InlineData(DateRangeType.Last7Days, "2019-02-26", "2019-03-05")]
        [InlineData(DateRangeType.Last14Days, "2019-02-19", "2019-03-05")]
        [InlineData(DateRangeType.Last30Days, "2019-02-03", "2019-03-05")]
        public async Task PostCreateViewModelAsync_ShouldUseCorrectTimespan(DateRangeType dateRangeType, string fromDate, string toDate)
        {
            var orchestrator = GetOrchestrator();

            string reportName = $"{DateTime.Parse(fromDate).AsGdsDate()} to {DateTime.Parse(toDate).AsGdsDate()}";

            var model = new ApplicationsReportCreateEditModel
            {
                DateRange = dateRangeType
            };

            var user = new VacancyUser();

            await orchestrator.PostCreateViewModelAsync(model, user);

            _client.Verify(c => c.CreateApplicationsReportAsync(
                DateTime.Parse(fromDate),
                DateTime.Parse(toDate).AddDays(1).AddTicks(-1),
                user,
                reportName),
                Times.Once);
        }

        [Fact]
        public async Task PostCreateViewModelAsync_ShouldUseCustomTimespan()
        {
            var model = new ApplicationsReportCreateEditModel
            {
                DateRange = DateRangeType.Custom,
                FromDay = "1",
                FromMonth = "2",
                FromYear = "2018",
                ToDay = "3",
                ToMonth = "4",
                ToYear = "2019",
            };

            var user = new VacancyUser();

            var orchestrator = GetOrchestrator();

            await orchestrator.PostCreateViewModelAsync(model, user);

            _client.Verify(c => c.CreateApplicationsReportAsync(
                    DateTime.Parse("2018-02-01"),
                    DateTime.Parse("2019-04-03").AddDays(1).AddTicks(-1),
                    user,
                    "01 Feb 2018 to 03 Apr 2019"),
                Times.Once);
        }

        private ApplicationsReportOrchestrator GetOrchestrator()
        {
            var timeProvider = new Mock<ITimeProvider>();

            var today = DateTime.Parse("2019-03-05").ToUniversalTime();
            timeProvider.Setup(t => t.Today).Returns(today);
            timeProvider.Setup(t => t.NextDay).Returns(today.AddDays(1));

            return new ApplicationsReportOrchestrator(_client.Object, timeProvider.Object);
        }
    }
}
