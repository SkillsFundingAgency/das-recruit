using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.ProviderVacancyClient
{
    public class ProviderVacancyClientGetDashboardSummaryTests
    {
        [Test, MoqAutoData]
        public async Task Then_Maps_Statuses_To_ProviderDashboardSummary(
            long ukprn,
            string userId,
            GetProviderDashboardApiResponse apiResponse,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<ITrainingProviderService> trainingProviderService,
            VacancyClient vacancyClient)
        {
            trainingProviderService.Setup(x => x.GetProviderDashboardStats(ukprn, userId)).ReturnsAsync(apiResponse);

            var actual = await vacancyClient.GetDashboardSummary(ukprn, userId);

            actual.Closed.Should().Be(apiResponse.ClosedVacanciesCount);
            actual.Draft.Should().Be(apiResponse.DraftVacanciesCount);
            actual.Review.Should().Be(apiResponse.ReviewVacanciesCount);
            actual.Referred.Should().Be(apiResponse.ReferredVacanciesCount);
            actual.Live.Should().Be(apiResponse.LiveVacanciesCount);
            actual.NumberOfNewApplications.Should().Be(apiResponse.NewApplicationsCount);
            actual.NumberOfUnsuccessfulApplications.Should().Be(apiResponse.UnsuccessfulApplicationsCount);
            actual.NumberOfSuccessfulApplications.Should().Be(apiResponse.SuccessfulApplicationsCount);
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

            trainingProviderService.Verify(x => x.GetProviderDashboardStats(ukprn, userId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_No_Vacancies(
            long ukprn,
            string userId,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<ITrainingProviderService> trainingProviderService,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = [],
                VacancyStatusDashboard = [],
                VacanciesClosingSoonWithNoApplications = 0,
                VacancySharedApplicationsDashboard = [],
            });

            trainingProviderService.Setup(x => x.GetProviderDashboardStats(ukprn, userId)).ReturnsAsync(new GetProviderDashboardApiResponse()
            {
                NewApplicationsCount = 0,
                EmployerReviewedApplicationsCount = 0,
                UnsuccessfulApplicationsCount = 0,
                SuccessfulApplicationsCount = 0,
            });

            var actual = await vacancyClient.GetDashboardSummary(ukprn, userId);

            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }


        [Test, MoqAutoData]
        public async Task Then_Gets_DashboardSummary_With_TransferredVacancies(
            List<TransferInfo> transferredVacancies,
            long ukprn,
            string userId,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetTransferredFromProviderAsync(ukprn)).ReturnsAsync(transferredVacancies);

            var actual = await vacancyClient.GetDashboardSummary(ukprn, userId);

            actual.TransferredVacancies.Should().BeEquivalentTo(transferredVacancies.Select(t =>
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                }));
        }
    }
}