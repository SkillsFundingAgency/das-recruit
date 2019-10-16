using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider;
using FluentAssertions;
using Moq;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class TrainingProviderSummaryProviderTests
    {
        [Fact]
        public async Task GetAsync_ShouldReturnEsfaTestProviderForUkrpn()
        {
            var providerClientMock = new Mock<IProviderApiClient>();
            var cache = new TestCache();
            var timeProviderMock = new Mock<ITimeProvider>();

            var sut = new TrainingProviderSummaryProvider(providerClientMock.Object, cache, timeProviderMock.Object);

            var provider = await sut.GetAsync(EsfaTestTrainingProvider.Ukprn);

            provider.Ukprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
            provider.ProviderName.Should().Be(EsfaTestTrainingProvider.Name);
            providerClientMock.Verify(c => c.FindAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAsync_ShouldAttemptToFindTrainingProvider()
        {
            var providerClientMock = new Mock<IProviderApiClient>();
            providerClientMock.Setup(p => p.FindAllAsync())
                .ReturnsAsync(new List<ProviderSummary>
                {
                    new ProviderSummary{Ukprn = 88888888, ProviderName = "provider 1"},
                    new ProviderSummary{Ukprn = 88888889, ProviderName = "provider 2"}
                });

            var cache = new TestCache();
            var timeProviderMock = new Mock<ITimeProvider>();

            var sut = new TrainingProviderSummaryProvider(providerClientMock.Object, cache, timeProviderMock.Object);

            var provider = await sut.GetAsync(88888888);

            providerClientMock.Verify(c => c.FindAllAsync(), Times.Once);

            provider.Ukprn.Should().Be(88888888);
            provider.ProviderName.Should().Be("provider 1");
        }
    }
}
