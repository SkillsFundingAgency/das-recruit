using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class VacancyHasStartedPartTwoTests
    {
        [Fact]
        public void ShouldReturnTrueIfAnyPartTwoFieldsAreCompleted()
        {
            Utility.VacancyHasStartedPartTwo(new Vacancy {EmployerDescription = "some value"}).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { ThingsToConsider = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContactName = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContactEmail = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { EmployerContactPhone = "some value" }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Qualifications = new List<Qualification>() }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Skills = new List<string>() }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { TrainingProvider = new TrainingProvider() }).Should().BeTrue();
            Utility.VacancyHasStartedPartTwo(new Vacancy { Description = "some value" }).Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalseIfPartTwoNotStarted()
        {
            Utility.VacancyHasStartedPartTwo(new Vacancy()).Should().BeFalse();
        }
    }
}
