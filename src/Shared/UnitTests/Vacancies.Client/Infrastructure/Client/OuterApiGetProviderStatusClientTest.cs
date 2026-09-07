using AutoFixture.NUnit3;
using Esfa.Recruit.UnitTests.TestHelpers;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client
{
    public class OuterApiGetProviderStatusClientTest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Made_And_ProviderResponse_Returned(
            long ukprn,
            ProviderAccountResponse apiResponse,
            [Frozen] Mock<IOuterApiClient> apiClient,
            [Frozen] Mock<ICache> cache,
            OuterApiGetProviderStatusClient service)
        {
            //Arrange
            var request = new GetProviderStatusDetails(ukprn);
            apiClient.Setup(x =>
                    x.Get<ProviderAccountResponse>(
                        It.Is<GetProviderStatusDetails>(c => c.GetUrl.Equals(request.GetUrl))))
                .ReturnsAsync(apiResponse);

            cache.Setup(x => x.CacheAsideAsync($"{CacheKeys.ProviderAccountsPermissions}_{ukprn}", It.IsAny<DateTime>(),
                It.IsAny<Func<Task<ProviderAccountResponse>>>())).ReturnsAsync(apiResponse);

            //Act
            var actual = await service.GetProviderStatus(ukprn);

            //Assert
            actual.CanAccessService.Should().Be(apiResponse.CanAccessService);
        }

        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Made_And_ProviderResponse_Returned_When_Cache_Is_Null(
            long ukprn,
            ProviderAccountResponse apiResponse,
            [Frozen] Mock<ITimeProvider> mockTimeProvider,
            [Frozen] Mock<IOuterApiClient> apiClient,
            [Frozen] Mock<ILogger<OuterApiGetProviderStatusClient>> logger,
            OuterApiGetProviderStatusClient service)
        {
            //Arrange
            var cache = new TestCache();
            service = new OuterApiGetProviderStatusClient(apiClient.Object, cache, mockTimeProvider.Object,
                logger.Object);

            var request = new GetProviderStatusDetails(ukprn);
            apiClient.Setup(x =>
                    x.Get<ProviderAccountResponse>(
                        It.Is<GetProviderStatusDetails>(c => c.GetUrl.Equals(request.GetUrl))))
                .ReturnsAsync(apiResponse);

            //Act
            var actual = await service.GetProviderStatus(ukprn);

            //Assert
            actual.CanAccessService.Should().Be(apiResponse.CanAccessService);
        }

        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Made_And_ProviderResponse_Returned_True_For_EsfaTestTrainingProvider(
            ProviderAccountResponse apiResponse,
            [Frozen] Mock<ITimeProvider> mockTimeProvider,
            [Frozen] Mock<IOuterApiClient> apiClient,
            [Frozen] Mock<ICache> cache,
            [Frozen] Mock<ILogger<OuterApiGetProviderStatusClient>> logger,
            OuterApiGetProviderStatusClient service)
        {
            //Act
            var actual = await service.GetProviderStatus(EsfaTestTrainingProvider.Ukprn);

            //Assert
            actual.CanAccessService.Should().BeTrue();
        }
    }
}