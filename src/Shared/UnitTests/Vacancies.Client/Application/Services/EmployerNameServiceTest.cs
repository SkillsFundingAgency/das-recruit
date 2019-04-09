using System;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services
{
    public class EmployerNameServiceTest
    {
        private Mock<IVacancyRepository> _mockVacancyRepository = new Mock<IVacancyRepository>();
        private Mock<IEmployerProfileRepository> _mockEmployerProfileRepository = new Mock<IEmployerProfileRepository>();

        [Theory]
        [InlineData(VacancyStatus.Live)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public void ShouldReturnVacancyEmployerName(VacancyStatus status)
        {
            var employerName = "Employer Name";
            var vacancy = new Vacancy()
            {
                Status = status,
                EmployerName = employerName
            };
            _mockVacancyRepository.Setup(vr => vr.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);

            var sut = GetSut();

            var result = sut.GetEmployerNameAsync(Guid.NewGuid()).Result;

            result.Should().Be(employerName);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public void ShouldReturnVacancyLegalEntityName(VacancyStatus status)
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
            _mockVacancyRepository.Setup(vr => vr.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);

            var sut = GetSut();

            var result = sut.GetEmployerNameAsync(Guid.NewGuid()).Result;

            result.Should().Be(legalEntityName);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public void ShouldReturnTradingName(VacancyStatus status)
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
            _mockVacancyRepository.Setup(vr => vr.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
            _mockEmployerProfileRepository.Setup(pr => pr.GetAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(profile);

            var sut = GetSut();

            var result = sut.GetEmployerNameAsync(Guid.NewGuid()).Result;

            result.Should().Be(tradingName);
        }

        private EmployerNameService GetSut()
        {
            return new EmployerNameService(_mockVacancyRepository.Object, _mockEmployerProfileRepository.Object);
        } 
    }
}