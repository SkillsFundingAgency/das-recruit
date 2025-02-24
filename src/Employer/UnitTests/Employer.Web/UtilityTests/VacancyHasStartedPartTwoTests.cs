using System.Collections.Generic;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class VacancyHasStartedPartTwoTests
    {
        private Utility _utility;
        public VacancyHasStartedPartTwoTests ()
        {
            _utility = new Utility(Mock.Of<IRecruitVacancyClient>());
        }
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
            _utility.VacancyHasStartedPartTwo(new Vacancy { Qualifications = new List<Qualification>() }).Should().BeTrue();
            _utility.VacancyHasStartedPartTwo(new Vacancy { Skills = new List<string>() }).Should().BeTrue();
            _utility.VacancyHasStartedPartTwo(new Vacancy { Description = "some value" }).Should().BeTrue();
        }

        [Test]
        public void ShouldReturnFalseIfPartTwoNotStarted()
        {
            _utility.VacancyHasStartedPartTwo(new Vacancy()).Should().BeFalse();
        }
    }
}
