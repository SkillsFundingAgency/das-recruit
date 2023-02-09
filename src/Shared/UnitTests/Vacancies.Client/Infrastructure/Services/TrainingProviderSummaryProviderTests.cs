using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class TrainingProviderSummaryProviderTests
    {
        [Fact]
        public async Task GetAsync_ShouldReturnEsfaTestProviderForUkrpn()
        {
            var providerClientMock = new Mock<ITrainingProviderService>();

            var sut = new TrainingProviderSummaryProvider(providerClientMock.Object);

            var provider = await sut.GetAsync(EsfaTestTrainingProvider.Ukprn);

            provider.Ukprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
            provider.ProviderName.Should().Be(EsfaTestTrainingProvider.Name);
            providerClientMock.Verify(c => c.FindAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAsync_ShouldAttemptToFindTrainingProvider()
        {
            var ukprn = 88888888;
            var providerClientMock = new Mock<ITrainingProviderService>();
            providerClientMock.Setup(p => p.GetProviderAsync(ukprn))
                .ReturnsAsync(new TrainingProvider
                {
                    Ukprn = 88888888, 
                    Name = "provider 1"
                });

            var sut = new TrainingProviderSummaryProvider(providerClientMock.Object);

            var provider = await sut.GetAsync(88888888);

            provider.Ukprn.Should().Be(88888888);
            provider.ProviderName.Should().Be("provider 1");
        }

        [Fact]
        public async Task GetAll_ShouldGetAllTrainingProviders()
        {
            var providerClientMock = new Mock<ITrainingProviderService>();
            providerClientMock.Setup(p => p.FindAllAsync())
                .ReturnsAsync(new List<TrainingProvider>
                {
                    new TrainingProvider
                    {
                        Ukprn = 88888888, 
                        Name = "provider 1"
                    },
                    new TrainingProvider
                    {
                        Ukprn = 88888888, 
                        Name = "provider 1"
                    }
                    
                });

            var sut = new TrainingProviderSummaryProvider(providerClientMock.Object);

            var providers = await sut.FindAllAsync();

            providers.Count().Should().Be(2);
            providers.All(c => c.Ukprn.Equals(88888888) && c.ProviderName.Equals("provider 1")).Should().BeTrue();
        }
    }
}
