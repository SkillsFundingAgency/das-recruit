using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class TrainingProviderServiceTests
    {
        [Fact]
        public async Task GetProviderAsync_ShouldReturnEsfaTestTrainingProvider()
        {
            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var referenceDataReader = new Mock<IReferenceDataReader>();
            var cache = new Mock<ICache>();
            var timeProvider = new Mock<ITimeProvider>();
            var outerApiClient = new Mock<IOuterApiClient>();

            var sut = new TrainingProviderService(loggerMock.Object, referenceDataReader.Object, cache.Object, timeProvider.Object, outerApiClient.Object);

            var provider = await sut.GetProviderAsync(EsfaTestTrainingProvider.Ukprn);

            provider.Ukprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
            provider.Name.Should().Be(EsfaTestTrainingProvider.Name);
            provider.Address.Postcode.Should().Be("CV1 2WT");

            referenceDataReader.Verify(p => p.GetReferenceData<TrainingProviders>(), Times.Never);
        }

        [Fact]
        public async Task GetProviderAsync_ShouldAttemptToFindTrainingProvider()
        {
            const long ukprn = 88888888;

            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var referenceDataReader = new Mock<IReferenceDataReader>();
            var cache = new Mock<ICache>();
            var timeProvider = new Mock<ITimeProvider>();
            var outerApiClient = new Mock<IOuterApiClient>();
            var trainingProvider = new TrainingProvider
            {
                Ukprn = ukprn,
                Name = "name",
                Address = new TrainingProviderAddress
                {
                    AddressLine1 = "address line 1",
                    AddressLine2  = "address line 2",
                    AddressLine3 = "address line 3",
                    AddressLine4 = "address line 4",
                    Postcode = "post code"
                }
            };
            var providers = new TrainingProviders
            {
                Data = new List<TrainingProvider>
                {
                    trainingProvider
                }
            };
            cache.Setup(x => x.CacheAsideAsync(CacheKeys.TrainingProviders, It.IsAny<DateTime>(),
                    It.IsAny<Func<Task<TrainingProviders>>>() ))
                .ReturnsAsync(providers);

            var sut = new TrainingProviderService(loggerMock.Object, referenceDataReader.Object, cache.Object, timeProvider.Object, outerApiClient.Object);

            var provider = await sut.GetProviderAsync(ukprn);

            
            provider.Name.Should().Be("name");
            provider.Ukprn.Should().Be(ukprn);
            provider.Address.AddressLine1.Should().Be("address line 1");
            provider.Address.AddressLine2.Should().Be("address line 2");
            provider.Address.AddressLine3.Should().Be("address line 3");
            provider.Address.AddressLine4.Should().Be("address line 4");
            provider.Address.Postcode.Should().Be("post code");
        }

        [Test, MoqAutoData]
        public async Task GetProviderDashboardApplicationReviewStats_Should_Return_As_Expected(
            long ukprn,
            List<long> vacancyReferences,
            List<ApplicationReviewStats> response,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] TrainingProviderService trainingProviderService)
        {
            var expectedGetUrl = new GetProviderApplicationReviewsCountApiRequest(ukprn, vacancyReferences);
            outerApiClient.Setup(x => x.Post<List<ApplicationReviewStats>>(
                It.Is<GetProviderApplicationReviewsCountApiRequest>(r => r.PostUrl == expectedGetUrl.PostUrl)))
                .ReturnsAsync(response);

            var result = await trainingProviderService.GetProviderDashboardApplicationReviewStats(ukprn, vacancyReferences);

            result.Should().BeEquivalentTo(response);
        }
    }
}
