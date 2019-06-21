using System.Collections.Generic;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests
{
    public class VacancyHasStartedPartTwoTests
    {
        [Fact]
        public void ShouldReturnTrueIfAnyPartTwoFieldsAreCompleted()
        {
            Utility.VacancyHasStartedPartTwo(new Vacancy { ShortDescription = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { EmployerDescription = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ThingsToConsider = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Name = "some value" }}).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Email = "some value"} }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ProviderContact = new ContactDetail { Phone = "some value"} }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Qualifications = new List<Qualification>() }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Skills = new List<string>() }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Description = "some value" }).Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalseIfPartTwoNotStarted()
        {
            Utility.VacancyHasStartedPartTwo(new Vacancy()).Should().BeFalse();
        }
    }
}
