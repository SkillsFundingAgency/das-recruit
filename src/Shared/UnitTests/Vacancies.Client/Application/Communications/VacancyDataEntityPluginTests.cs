using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications
{
    public class VacancyDataEntityPluginTests
    {
        private readonly Mock<IVacancyRepository> _mockRepository = new Mock<IVacancyRepository>();
        private readonly Fixture _fixture = new Fixture();
        private VacancyDataEntityPlugin GetSut() => new VacancyDataEntityPlugin(_mockRepository.Object);

        [Theory]
        [InlineData(EmployerNameOption.TradingName)]
        [InlineData(EmployerNameOption.RegisteredName)]
        [InlineData(EmployerNameOption.Anonymous)]
        public async Task ShouldReturnThreeDataItems(EmployerNameOption employerNameOption)
        {
            var vacancy = _fixture
                .Build<Vacancy>()
                .With(r => r.EmployerNameOption, employerNameOption)
                .Create();
            _mockRepository
                .Setup(r => r.GetVacancyAsync(It.IsAny<long>()))
                .ReturnsAsync(vacancy);

            var expectedEmployerName = employerNameOption == EmployerNameOption.TradingName ? vacancy.EmployerName : vacancy.LegalEntityName;

            var sut = GetSut();

            var dataItems = await sut.GetDataItemsAsync(_fixture.Create<long>());

            dataItems.Count().Should().Be(3);
            dataItems.First(t => t.Key == DataItemKeys.Vacancy.VacancyReference).Value.Should().Be(vacancy.VacancyReference.ToString());
            dataItems.First(t => t.Key == DataItemKeys.Vacancy.VacancyTitle).Value.Should().Be(vacancy.Title);
            dataItems.First(t => t.Key == DataItemKeys.Vacancy.EmployerName).Value.Should().Be(expectedEmployerName);
        }
    }
}