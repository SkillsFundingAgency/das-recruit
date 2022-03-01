using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class VacancyHasCompletedTaskList
    {
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Not_Specified_Then_Not_Completed(Vacancy vacancy, Utility utility)
        {
            vacancy.ApplicationMethod = null;

            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Specified_Then_Completed(Vacancy vacancy, Utility utility)
        {
            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeTrue();
        }
    }
}