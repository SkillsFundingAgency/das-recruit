using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services.EmployerService
{
    public class GetEmployerNameTests
    {
        private Mock<IEmployerProfileRepository> _mockEmployerProfileRepository = new Mock<IEmployerProfileRepository>();

        [Theory]
        [InlineData(VacancyStatus.Live)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public async Task GetEmployerNameAsync_ShouldReturnVacancyEmployerName(VacancyStatus status)
        {
            var employerName = "Employer Name";
            var vacancy = new Vacancy()
            {
                Status = status,
                EmployerName = employerName
            };
            
            var sut = GetSut();

            var result = await sut.GetEmployerNameAsync(vacancy);

            result.Should().Be(employerName);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public async Task GetEmployerNameAsync_ShouldReturnVacancyLegalEntityName(VacancyStatus status)
        {
            var employerName = "Employer Name";
            var legalEntityName = "Legal Entity Name";
            var vacancy = new Vacancy()
            {
                Status = status,
                EmployerName = employerName,
                LegalEntityName = legalEntityName,
                EmployerNameOption = EmployerNameOption.RegisteredName
            };

            var sut = GetSut();

            var result = await sut.GetEmployerNameAsync(vacancy);

            result.Should().Be(legalEntityName);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public async Task GetEmployerNameAsync_ShouldReturnTradingName(VacancyStatus status)
        {
            var employerName = "Employer Name";
            var legalEntityName = "Legal Entity Name";
            var tradingName = "Trading Name";
            var vacancy = new Vacancy()
            {
                Status = status,
                EmployerName = employerName,
                LegalEntityName = legalEntityName,
                EmployerNameOption = EmployerNameOption.TradingName
            };

            var profile = new EmployerProfile
            {
                TradingName = tradingName
            };
            
            _mockEmployerProfileRepository.Setup(pr => pr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profile);

            var sut = GetSut();

            var result = await sut.GetEmployerNameAsync(vacancy);

            result.Should().Be(tradingName);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public async Task GetEmployerNameAsync_ShouldReturnVacancyEmployerNameForAnonymous(VacancyStatus status)
        {
            var employerName = "Employer Name";
            var vacancy = new Vacancy() {
                Status = status,
                EmployerName = employerName,
                EmployerNameOption = EmployerNameOption.Anonymous
            };

            var sut = GetSut();

            var result = await sut.GetEmployerNameAsync(vacancy);

            result.Should().Be(employerName);
        }

        private Recruit.Vacancies.Client.Application.Services.EmployerService GetSut()
        {
            return new Recruit.Vacancies.Client.Application.Services.EmployerService(_mockEmployerProfileRepository.Object);
        }
    }
}