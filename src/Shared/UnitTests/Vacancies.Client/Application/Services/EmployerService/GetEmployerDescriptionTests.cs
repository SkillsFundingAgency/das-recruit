using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services.EmployerService
{
    public class GetEmployerDescriptionTests
    {
        private Mock<IEmployerProfileRepository> _mockEmployerProfileRepository = new Mock<IEmployerProfileRepository>();

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        public async Task GetEmployerDescriptionAsync_ShouldReturnEmployerProfileAboutOrganisation(VacancyStatus status)
        {
            var employerProfileAboutOrganisation = "Employer Profile About Organisation";
            
            var vacancy = new Vacancy() {
                Status = status,
                EmployerDescription = "Employer Description",
            };

            var profile = new EmployerProfile {
                AboutOrganisation = employerProfileAboutOrganisation
            };

            _mockEmployerProfileRepository.Setup(pr => pr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profile);

            var sut = GetSut();

            var result = await sut.GetEmployerDescriptionAsync(vacancy);

            result.Should().Be(employerProfileAboutOrganisation);
        }

        [Theory]
        [InlineData(VacancyStatus.Live)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public async Task GetEmployerDescriptionAsync_ShouldReturnVacancyEmployerDescription(VacancyStatus status)
        {
            var employerDescription = "Employer Description";

            var vacancy = new Vacancy() {
                Status = status,
                EmployerDescription = "Employer Description"
            };

            var sut = GetSut();

            var result = await sut.GetEmployerDescriptionAsync(vacancy);

            result.Should().Be(employerDescription);
        }

        private Recruit.Vacancies.Client.Application.Services.EmployerService GetSut()
        {
            return new Recruit.Vacancies.Client.Application.Services.EmployerService(_mockEmployerProfileRepository.Object);
        }
    }
}