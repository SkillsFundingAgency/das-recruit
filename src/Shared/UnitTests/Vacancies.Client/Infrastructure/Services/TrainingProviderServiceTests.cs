using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    [TestFixture]
    public class TrainingProviderServiceTests
    {
        [Test]
        public async Task GetProviderAsync_ShouldReturnEsfaTestTrainingProvider()
        {
            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var referenceDataReader = new Mock<IReferenceDataReader>();
            var outerApiClient = new Mock<IOuterApiClient>();

            var sut = new TrainingProviderService(loggerMock.Object, outerApiClient.Object);

            var provider = await sut.GetProviderAsync(EsfaTestTrainingProvider.Ukprn);

            provider.Ukprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
            provider.Name.Should().Be(EsfaTestTrainingProvider.Name);
            provider.Address.Postcode.Should().Be("CV1 2WT");

            referenceDataReader.Verify(p => p.GetReferenceData<TrainingProviders>(), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task GetProviderAsync_ShouldAttemptToFindTrainingProvider(
            GetProviderResponseItem response)
        {
            const long ukprn = 88888888;

            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var outerApiClient = new Mock<IOuterApiClient>();
            response.Ukprn = ukprn;
            

            var expectedGetUrl = new GetProviderRequest(ukprn);
            outerApiClient.Setup(x => x.Get<GetProviderResponseItem>(
                    It.Is<GetProviderRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
                .ReturnsAsync(response);

            var sut = new TrainingProviderService(loggerMock.Object, outerApiClient.Object);

            var provider = await sut.GetProviderAsync(ukprn);

            
            provider.Name.Should().Be(response.Name);
            provider.Ukprn.Should().Be(ukprn);
            provider.Address.AddressLine1.Should().Be(response.Address.Address1);
            provider.Address.AddressLine2.Should().Be(response.Address.Address2);
            provider.Address.AddressLine3.Should().Be(response.Address.Address3);
            provider.Address.AddressLine4.Should().Be(response.Address.Town);
            provider.Address.Postcode.Should().Be(response.Address.Postcode);
        }

        [Test, MoqAutoData]
        public async Task GetProviderDashboardApplicationReviewStats_Should_Return_As_Expected(
            long ukprn,
            List<long> vacancyReferences,
            GetApplicationReviewStatsResponse response,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] TrainingProviderService trainingProviderService)
        {
            var expectedGetUrl = new GetProviderApplicationReviewsCountApiRequest(ukprn, vacancyReferences);
            outerApiClient.Setup(x => x.Post<GetApplicationReviewStatsResponse>(
                It.Is<GetProviderApplicationReviewsCountApiRequest>(r => r.PostUrl == expectedGetUrl.PostUrl)))
                .ReturnsAsync(response);

            var result = await trainingProviderService.GetProviderDashboardApplicationReviewStats(ukprn, vacancyReferences);

            result.Should().BeEquivalentTo(response);
        }
        
        [Test, MoqAutoData]
        public async Task GetProviderDashboardVacanciesByApplicationReviewStatuses_Should_Return_As_Expected(
            long ukprn,
            int pageNumber,
            int pageSize,
            List<ApplicationReviewStatus> status,
            GetVacanciesDashboardResponse response,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] TrainingProviderService trainingProviderService)
        {
            var expectedGetUrl = new GetProviderDashboardVacanciesApiRequest(ukprn, pageNumber, pageSize, status);
            outerApiClient.Setup(x => x.Get<GetVacanciesDashboardResponse>(
                    It.Is<GetProviderDashboardVacanciesApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
                .ReturnsAsync(response);

            var result = await trainingProviderService.GetProviderDashboardVacanciesByApplicationReviewStatuses(ukprn, status, pageNumber, pageSize);

            result.Should().BeEquivalentTo(response);
        }

        [Test, MoqAutoData]
        public async Task GetProviderDashboardStats_Should_Return_As_Expected(
            long ukprn,
            string userId,
            GetProviderDashboardApiResponse response,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] TrainingProviderService trainingProviderService)
        {
            var expectedGetUrl = new GetProviderDashboardCountApiRequest(ukprn, userId);
            outerApiClient.Setup(x => x.Get<GetProviderDashboardApiResponse> (
                    It.Is<GetProviderDashboardCountApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
                .ReturnsAsync(response);

            var result = await trainingProviderService.GetProviderDashboardStats(ukprn, userId);

            result.Should().BeEquivalentTo(response);
        }
    }
}