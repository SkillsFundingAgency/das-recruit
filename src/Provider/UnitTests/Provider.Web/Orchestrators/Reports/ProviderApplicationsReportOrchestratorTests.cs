using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Reports
{
    public class ProviderApplicationsReportOrchestratorTests
    {
        [Theory]
        [InlineData(DateRangeType.Last7Days, "2019-02-26Z", "2019-03-05Z")] //GMT
        [InlineData(DateRangeType.Last14Days, "2019-02-19Z", "2019-03-05Z")] //GMT
        [InlineData(DateRangeType.Last30Days, "2019-02-03Z", "2019-03-05Z")] //GMT
        [InlineData(DateRangeType.Last7Days, "2019-04-01Z", "2019-04-08Z")] //BST
        [InlineData(DateRangeType.Last14Days, "2019-04-01Z", "2019-04-15Z")] //BST
        [InlineData(DateRangeType.Last30Days, "2019-04-01Z", "2019-05-01Z")] //BST
        public async Task PostCreateViewModelAsync_ShouldUseCorrectTimespan(DateRangeType dateRangeType, string fromDate, string toDate)
        {
            var mockClient = new Mock<IProviderVacancyClient>();
            var orchestrator = GetOrchestrator(mockClient.Object, $"{toDate}");

            var fromDateUtc = DateTimeOffset.Parse(fromDate).UtcDateTime;
            var toDateUtc = DateTimeOffset.Parse(toDate).UtcDateTime;

            long ukprn = 12345678;
            
            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
                DateRange = dateRangeType
            };

            var user = new VacancyUser();

            await orchestrator.PostCreateViewModelAsync(model, user);

            string expectedReportName = $"{fromDateUtc.AsGdsDate()} to {toDateUtc.AsGdsDate()}";

            mockClient.Verify(c => c.CreateProviderApplicationsReportAsync(
                ukprn,
                fromDateUtc,
                toDateUtc.AddDays(1).AddTicks(-1), 
                user,
                expectedReportName, VacancyType.Traineeship), 
                Times.Once);
        }

        [Theory]
        [InlineData("1", "2", "2018", "20", "2", "2019")] //GMT
        [InlineData("1", "4", "2018", "20", "4", "2019")] //BST
        public async Task PostCreateViewModelAsync_ShouldUseCustomTimespan_With_Correct_VacancyType(string fromDay, string fromMonth, string fromYear, string toDay, string toMonth, string toYear)
        {
            
            long ukprn = 12345678;

            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
                DateRange = DateRangeType.Custom,
                FromDay = fromDay,
                FromMonth = fromMonth,
                FromYear = fromYear,
                ToDay = toDay,
                ToMonth = toMonth,
                ToYear = toYear,
            };

            var user = new VacancyUser();

            var mockClient = new Mock<IProviderVacancyClient>();
            var orchestrator = GetOrchestrator(mockClient.Object, "2019-12-05Z");

            var fromDateUtc = DateTimeOffset.Parse($"{fromYear}-{fromMonth}-{fromDay}Z").UtcDateTime;
            var toDateUtc = DateTimeOffset.Parse($"{toYear}-{toMonth}-{toDay}Z").UtcDateTime;

            await orchestrator.PostCreateViewModelAsync(model, user);

            string expectedReportName = $"{fromDateUtc.AsGdsDate()} to {toDateUtc.AsGdsDate()}";

            mockClient.Verify(c => c.CreateProviderApplicationsReportAsync(
                    ukprn,
                    fromDateUtc,
                    toDateUtc.AddDays(1).AddTicks(-1),
                    user,
                    expectedReportName, VacancyType.Traineeship),
                Times.Once);
        }
        
        private ProviderApplicationsReportOrchestrator GetOrchestrator(IProviderVacancyClient client, string todayDate)
        {
            var timeProvider = new Mock<ITimeProvider>();

            var today = DateTimeOffset.Parse(todayDate).UtcDateTime;
            timeProvider.Setup(t => t.Today).Returns(today);
            timeProvider.Setup(t => t.NextDay).Returns(today.AddDays(1));

            return new ProviderApplicationsReportOrchestrator(client, timeProvider.Object, new ServiceParameters("Traineeship"));
        }
    }
}
