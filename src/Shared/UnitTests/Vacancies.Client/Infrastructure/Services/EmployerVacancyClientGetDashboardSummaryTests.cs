using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class EmployerVacancyClientGetDashboardSummaryTests
    {
        [Test, MoqAutoData]
        public async Task Then_Maps_Statuses_To_EmployerDashboardSummary(
            string employerAccountId,
            string userId,
            GetEmployerDashboardApiResponse apiResponse,
            GetAlertsByAccountIdApiResponse alertsResponse,
            [Frozen] Mock<IEmployerAccountProvider> employerAccountProvider,
            VacancyClient vacancyClient)
        {
            employerAccountProvider.Setup(x => x.GetEmployerDashboardStats(employerAccountId))
                .ReturnsAsync(apiResponse);
            employerAccountProvider.Setup(x => x.GetEmployerAlerts(employerAccountId, userId))
                .ReturnsAsync(alertsResponse);

            var actual = await vacancyClient.GetDashboardSummary(employerAccountId, userId);

            actual.Closed.Should().Be(apiResponse.ClosedVacanciesCount);
            actual.Draft.Should().Be(apiResponse.DraftVacanciesCount);
            actual.Review.Should().Be(apiResponse.ReviewVacanciesCount);
            actual.Referred.Should().Be(apiResponse.ReferredVacanciesCount);
            actual.Live.Should().Be(apiResponse.LiveVacanciesCount);
            actual.NumberOfNewApplications.Should().Be(apiResponse.NewApplicationsCount);
            actual.NumberOfUnsuccessfulApplications.Should().Be(apiResponse.UnsuccessfulApplicationsCount);
            actual.NumberOfSuccessfulApplications.Should().Be(apiResponse.SuccessfulApplicationsCount);
            actual.NumberOfSharedApplications.Should().Be(apiResponse.SharedApplicationsCount);
            actual.NumberClosingSoon.Should().Be(apiResponse.ClosingSoonVacanciesCount);
            actual.NumberClosingSoonWithNoApplications.Should().Be(apiResponse.ClosingSoonWithNoApplications);
            actual.HasVacancies.Should().BeTrue();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeTrue();
            actual.NumberOfVacancies.Should()
                .Be(apiResponse.ClosedVacanciesCount +
                    apiResponse.DraftVacanciesCount +
                    apiResponse.ReviewVacanciesCount +
                    apiResponse.ReferredVacanciesCount +
                    apiResponse.LiveVacanciesCount +
                    apiResponse.SubmittedVacanciesCount);
            actual.EmployerRevokedTransferredVacanciesAlert.Should()
                .BeEquivalentTo(alertsResponse.EmployerRevokedTransferredVacanciesAlert);
            actual.BlockedProviderAlert.Should().BeEquivalentTo(alertsResponse.BlockedProviderAlert);
            actual.WithDrawnByQaVacanciesAlert.Should()
                .BeEquivalentTo(alertsResponse.WithDrawnByQaVacanciesAlert);
            actual.BlockedProviderTransferredVacanciesAlert.Should()
                .BeEquivalentTo(alertsResponse.BlockedProviderTransferredVacanciesAlert);

            employerAccountProvider.Verify(x => x.GetEmployerDashboardStats(employerAccountId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_No_Vacancies(
            string employerAccountId,
            string userId,
            GetAlertsByAccountIdApiResponse alertsResponse,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<IEmployerAccountProvider> employerAccountProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = [],
                VacancyStatusDashboard = [],
                VacancySharedApplicationsDashboard = []
            });
            employerAccountProvider.Setup(x => x.GetEmployerDashboardStats(employerAccountId))
                .ReturnsAsync(new GetEmployerDashboardApiResponse
                {
                    NewApplicationsCount = 0,
                    UnsuccessfulApplicationsCount = 0,
                    SuccessfulApplicationsCount = 0,
                    SharedApplicationsCount = 0,
                    EmployerReviewedApplicationsCount = 0
                });
            employerAccountProvider.Setup(x => x.GetEmployerAlerts(employerAccountId, userId))
                .ReturnsAsync(alertsResponse);

            var actual = await vacancyClient.GetDashboardSummary(employerAccountId, userId);
            
            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }
    }
}