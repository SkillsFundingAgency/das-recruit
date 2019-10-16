using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class TrainingProviderServiceTests
    {
        [Fact]
        public async Task GetProviderAsync_ShouldReturnEsfaTestTrainingProvider()
        {
            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var providerApiMock = new Mock<IProviderApiClient>();

            var sut = new TrainingProviderService(loggerMock.Object, providerApiMock.Object);

            var provider = await sut.GetProviderAsync(EsfaTestTrainingProvider.Ukprn);

            provider.Ukprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
            provider.Name.Should().Be(EsfaTestTrainingProvider.Name);
            provider.Address.Postcode.Should().Be("CV1 2WT");

            providerApiMock.Verify(p => p.GetAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task GetProviderAsync_ShouldAttemptToFindTrainingProvider()
        {
            const long ukprn = 88888888;

            var loggerMock = new Mock<ILogger<TrainingProviderService>>();
            var providerApiMock = new Mock<IProviderApiClient>();
            providerApiMock.Setup(p => p.GetAsync(ukprn))
                .ReturnsAsync(new Provider
                {
                    Ukprn = ukprn,
                    ProviderName = "name",
                    Addresses = new List<ContactAddress>
                    {
                        new ContactAddress
                        {
                            ContactType = "something else",
                            Primary = "ignored",
                            Secondary = "ignored",
                            Street = "ignored",
                            Town = "ignored",
                            PostCode = "ignored"
                        },
                        new ContactAddress
                        {
                            ContactType = "PRIMARY",
                            Primary = "address line 1",
                            Secondary = "address line 2",
                            Street = "address line 3",
                            Town = "address line 4",
                            PostCode = "post code"
                        },
                    }
                }); ;

            var sut = new TrainingProviderService(loggerMock.Object, providerApiMock.Object);

            var provider = await sut.GetProviderAsync(ukprn);

            providerApiMock.Verify(p => p.GetAsync(ukprn), Times.Once);

            provider.Name.Should().Be("name");
            provider.Ukprn.Should().Be(ukprn);
            provider.Address.AddressLine1.Should().Be("address line 1");
            provider.Address.AddressLine2.Should().Be("address line 2");
            provider.Address.AddressLine3.Should().Be("address line 3");
            provider.Address.AddressLine4.Should().Be("address line 4");
            provider.Address.Postcode.Should().Be("post code");
        }
    }
}
