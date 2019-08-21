using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
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
    public class ApprenticeshipServiceDataEntityPluginTests
    {
        private readonly Fixture _fixture = new Fixture();
        const string EmployerUrl = "https://www.google.com/";
        const string ProviderUrl = "https://www.bing.com/";

        private readonly Mock<IOptions<CommunicationsConfiguration>> _mockOptions = new Mock<IOptions<CommunicationsConfiguration>>();
        private readonly Mock<IVacancyRepository> _mockRepository = new Mock<IVacancyRepository>();

        private ApprenticeshipServiceDataEntityPlugin GetSut() => new ApprenticeshipServiceDataEntityPlugin(_mockRepository.Object, _mockOptions.Object);

        public ApprenticeshipServiceDataEntityPluginTests()
        {
            _mockOptions.Setup(m => m.Value).Returns(new CommunicationsConfiguration{EmployersApprenticeshipServiceUrl = EmployerUrl, ProvidersApprenticeshipServiceUrl = ProviderUrl});
        }

        [Theory]
        [InlineData(OwnerType.Employer)]
        [InlineData(OwnerType.Provider)]
        public async Task UrlShouldMatchOwnerType(OwnerType owner)
        {
            var vacancy = new Vacancy
            { 
                OwnerType = owner,
                EmployerAccountId = _fixture.Create<string>(),
                TrainingProvider = new TrainingProvider { Ukprn = _fixture.Create<long>() }
            };

            var expectedUrl = owner == OwnerType.Employer 
                ? $"{EmployerUrl}{vacancy.EmployerAccountId}" 
                : $"{ProviderUrl}{vacancy.TrainingProvider.Ukprn}/vacancies";

            _mockRepository
                .Setup(r => r.GetVacancyAsync(It.IsAny<long>()))
                .ReturnsAsync(vacancy);
            
            var sut = GetSut();

            var dataItems = await sut.GetDataItemsAsync(_fixture.Create<long>());

            dataItems.Count().Should().Be(2);

            dataItems.First(d => d.Key == DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUrl).Value.Should().Be(expectedUrl);
            dataItems.First(d => d.Key == DataItemKeys.ApprenticeshipService.HelpdeskPhoneNumber).Value.Should().Be(CommunicationConstants.HelpdeskPhoneNumber);
        }

    }
}