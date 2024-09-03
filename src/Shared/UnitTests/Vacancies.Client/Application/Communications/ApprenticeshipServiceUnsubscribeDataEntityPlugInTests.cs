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

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications;

public class ApprenticeshipServiceUnsubscribeDataEntityPlugInTests
{
    private readonly Fixture _fixture = new Fixture();
    const string EmployerUrl = "https://www.google.com/";
    const string ProviderUrl = "https://www.bing.com/";

    private readonly Mock<IOptions<CommunicationsConfiguration>> _mockOptions = new Mock<IOptions<CommunicationsConfiguration>>();
    private readonly Mock<IVacancyRepository> _mockRepository = new Mock<IVacancyRepository>();

    private ApprenticeshipServiceUnsubscribeDataEntityPlugIn GetSut() => new ApprenticeshipServiceUnsubscribeDataEntityPlugIn(_mockRepository.Object, _mockOptions.Object);

    public ApprenticeshipServiceUnsubscribeDataEntityPlugInTests()
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
            ? $"{EmployerUrl}{vacancy.EmployerAccountId}/notifications-manage"
            : $"{ProviderUrl}{vacancy.TrainingProvider.Ukprn}/notifications-manage";

        _mockRepository
            .Setup(r => r.GetVacancyAsync(It.IsAny<long>()))
            .ReturnsAsync(vacancy);

        var sut = GetSut();

        var dataItems = await sut.GetDataItemsAsync(_fixture.Create<long>());

        dataItems.Count().Should().Be(1);

        dataItems.Single(d => d.Key == CommunicationConstants.DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUnsubscribeUrl).Value.Should().Be(expectedUrl);
    }

}