using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services.EmployerService;

public class GetEmployerNameTests
{
    private readonly Mock<IEmployerProfileService> _mockEmployerProfileService = new();

    [Test]
    [TestCase(VacancyStatus.Live)]
    [TestCase(VacancyStatus.Submitted)]
    [TestCase(VacancyStatus.Approved)]
    [TestCase(VacancyStatus.Closed)]
    public async Task GetEmployerNameAsync_ShouldReturnVacancyEmployerName(VacancyStatus status)
    {
        var employerName = "Employer Name";
        var vacancy = new Vacancy
        {
            Status = status,
            EmployerName = employerName
        };
            
        var sut = GetSut();

        var result = await sut.GetEmployerNameAsync(vacancy);

        result.Should().Be(employerName);
    }

    [Test]
    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
    public async Task GetEmployerNameAsync_ShouldReturnVacancyLegalEntityName(VacancyStatus status)
    {
        var employerName = "Employer Name";
        var legalEntityName = "Legal Entity Name";
        var vacancy = new Vacancy
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

    [Test]
    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
    public async Task GetEmployerNameAsync_ShouldReturnTradingName(VacancyStatus status)
    {
        var employerName = "Employer Name";
        var legalEntityName = "Legal Entity Name";
        var tradingName = "Trading Name";
        var vacancy = new Vacancy
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
            
        _mockEmployerProfileService.Setup(pr => pr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profile);

        var sut = GetSut();

        var result = await sut.GetEmployerNameAsync(vacancy);

        result.Should().Be(tradingName);
    }

    [Test]
    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
    public async Task GetEmployerNameAsync_ShouldReturnVacancyEmployerNameForAnonymous(VacancyStatus status)
    {
        var employerName = "Employer Name";
        var vacancy = new Vacancy 
        {
            Status = status,
            EmployerName = employerName,
            EmployerNameOption = EmployerNameOption.Anonymous
        };

        var sut = GetSut();

        var result = await sut.GetEmployerNameAsync(vacancy);

        result.Should().Be(employerName);
    }

    private Recruit.Vacancies.Client.Application.Services.EmployerService GetSut() => new(_mockEmployerProfileService.Object);
}
