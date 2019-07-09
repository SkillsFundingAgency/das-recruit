using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace UnitTests.Vacancies.Client.Application.Communications
{
    public class ApprenticeshipServiceUrlPluginTests
    {
        private readonly Fixture _fixture = new Fixture();
        const string EmployerUrl = nameof(EmployerUrl);
        const string ProviderUrl = nameof(ProviderUrl);

        private readonly Mock<IOptions<CommunicationsConfiguration>> _mockOptions = new Mock<IOptions<CommunicationsConfiguration>>();
        private readonly Mock<IVacancyRepository> _mockRepository = new Mock<IVacancyRepository>();

        private ApprenticeshipServiceUrlPlugin GetSut() => new ApprenticeshipServiceUrlPlugin(_mockRepository.Object, _mockOptions.Object);

        public ApprenticeshipServiceUrlPluginTests()
        {
            _mockOptions.Setup(m => m.Value).Returns(new CommunicationsConfiguration{EmployersApprenticeshipServiceUrl = EmployerUrl, ProvidersApprenticeshipServiceUrl = ProviderUrl});
        }

        [Theory]
        [InlineData(OwnerType.Employer, EmployerUrl)]
        [InlineData(OwnerType.Provider, ProviderUrl)]
        public async Task UrlShouldMatchOwnerType(OwnerType owner, string expectedUrl)
        {
            _mockRepository
                .Setup(r => r.GetVacancyAsync(It.IsAny<long>()))
                .ReturnsAsync(new Vacancy{ OwnerType = owner});
            
            var sut = GetSut();

            var dataItems = await sut.GetDataItemsAsync(_fixture.Create<long>());

            dataItems.Count().Should().Be(1);

            dataItems.Single().Key.Should().Be(DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUrl);
            dataItems.Single().Value.Should().Be(expectedUrl);
        }

    }
}