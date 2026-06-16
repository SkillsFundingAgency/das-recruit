using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests;

public class VacancyHasStartedPartTwoTests
{
    private readonly Utility _utility = new(Mock.Of<IRecruitVacancyClient>(), Mock.Of<ITaskListValidator>());

    [Test]
    public void ShouldReturnTrueIfAnyPartTwoFieldsAreCompleted()
    {
        _utility.VacancyHasStartedPartTwo(new Vacancy { ShortDescription = "some value" }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { EmployerDescription = "some value" }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { ThingsToConsider = "some value" }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContact = new ContactDetail { Name = "some value" }}).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContact = new ContactDetail { Email = "some value"} }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContact = new ContactDetail { Phone = "some value"} }).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { Qualifications = []}).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { Skills = []}).Should().BeTrue();
        _utility.VacancyHasStartedPartTwo(new Vacancy { Description = "some value" }).Should().BeTrue();
    }

    [Test]
    public void ShouldReturnFalseIfPartTwoNotStarted()
    {
        _utility.VacancyHasStartedPartTwo(new Vacancy()).Should().BeFalse();
    }
}