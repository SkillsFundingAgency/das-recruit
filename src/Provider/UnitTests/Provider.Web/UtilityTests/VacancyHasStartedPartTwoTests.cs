using System.Collections.Generic;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests;

public class VacancyHasStartedPartTwoTests
{
    [Test, MoqAutoData]
    public void ShouldReturnTrueIfAnyPartTwoFieldsAreCompleted(Utility utility)
    {
        utility.VacancyHasStartedPartTwo(new Vacancy { EmployerDescription = "some value" }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ShortDescription = "some value" }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ThingsToConsider = "some value" }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Name = "some value" }}).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Email = "some value"} }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Phone = "some value"} }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { Qualifications = new List<Qualification>() }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { Skills = new List<string>() }).Should().BeTrue();
        utility.VacancyHasStartedPartTwo(new Vacancy { Description = "some value" }).Should().BeTrue();
    }

    [Test, MoqAutoData]
    public void ShouldReturnFalseIfPartTwoNotStarted(Utility utility)
    {
        utility.VacancyHasStartedPartTwo(new Vacancy()).Should().BeFalse();
    }
}