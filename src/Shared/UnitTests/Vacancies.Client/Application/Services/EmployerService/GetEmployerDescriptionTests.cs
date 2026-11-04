using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services.EmployerService;

public class GetEmployerDescriptionTests
{
    private readonly Mock<IEmployerProfileService> _mockEmployerProfileService = new();

    [Test]
    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
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

        _mockEmployerProfileService.Setup(pr => pr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profile);

        var sut = GetSut();

        var result = await sut.GetEmployerDescriptionAsync(vacancy);

        result.Should().Be(employerProfileAboutOrganisation);
    }

    [Test]
    [TestCase(VacancyStatus.Live)]
    [TestCase(VacancyStatus.Submitted)]
    [TestCase(VacancyStatus.Approved)]
    [TestCase(VacancyStatus.Closed)]
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

    private Recruit.Vacancies.Client.Application.Services.EmployerService GetSut() => new(_mockEmployerProfileService.Object);
}
