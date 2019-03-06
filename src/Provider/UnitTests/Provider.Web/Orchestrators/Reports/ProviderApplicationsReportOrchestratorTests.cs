using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Reports
{
    public class ProviderApplicationsReportOrchestratorTests
    {
        private readonly Mock<IProviderVacancyClient> _client = new Mock<IProviderVacancyClient>();

        [Theory]
        [InlineData(DateRangeType.Last7Days, "27-02-2019", "06-03-2019")]
        [InlineData(DateRangeType.Last14Days, "20-02-2019", "06-03-2019")]
        [InlineData(DateRangeType.Last30Days, "04-02-2019", "06-03-2019")]
        public async Task PostCreateViewModelAsync_ShouldUseCorrectTimespan(DateRangeType dateRangeType, string fromDate, string toDate)
        {
            var orchestrator = GetOrchestrator();

            long ukprn = 12345678;

            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
                DateRange = dateRangeType
            };

            var user = new VacancyUser();

            await orchestrator.PostCreateViewModelAsync(model, user);

            _client.Verify(c => c.CreateProviderApplicationsReportAsync(
                ukprn, 
                DateTime.Parse(fromDate), 
                DateTime.Parse(toDate), user), 
                Times.Once);
        }

        [Fact]
        public async Task PostCreateViewModelAsync_ShouldUseCustomTimespan()
        {
            long ukprn = 12345678;

            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
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

            _client.Verify(c => c.CreateProviderApplicationsReportAsync(
                    ukprn,
                    DateTime.Parse("2018-02-01"),
                    DateTime.Parse("2019-04-04"), user),
                Times.Once);
        }

        private ProviderApplicationsReportOrchestrator GetOrchestrator()
        {
            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(t => t.NextDay).Returns(DateTime.Parse("2019-03-06"));

            return new ProviderApplicationsReportOrchestrator(_client.Object, timeProvider.Object);
        }
    }
}
