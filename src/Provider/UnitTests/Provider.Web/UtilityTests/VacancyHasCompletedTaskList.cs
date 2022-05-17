using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests
{
    public class VacancyHasCompletedTaskList
    {
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Not_Specified_Then_Not_Completed(Vacancy vacancy, Utility utility)
        {
            vacancy.VacancyType = VacancyType.Apprenticeship;
            vacancy.ApplicationMethod = null;

            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Specified_Then_Completed(Vacancy vacancy, Utility utility)
        {
            vacancy.VacancyType = VacancyType.Apprenticeship;
            
            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeTrue();
        }
        
        
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Specified_For_Traineeship_Then_Not_Completed_When_No_Info(Vacancy vacancy, Utility utility)
        {
            vacancy.VacancyType = VacancyType.Traineeship;
            vacancy.ApplicationMethod = ApplicationMethod.ThroughFindATraineeship;
            vacancy.EmployerDescription = null;
            
            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void Then_If_Application_Method_Specified_For_Traineeship_Then_Completed_When_Info(Vacancy vacancy, Utility utility)
        {
            vacancy.VacancyType = VacancyType.Traineeship;
            vacancy.ApplicationMethod = ApplicationMethod.ThroughFindATraineeship;
            
            var actual = utility.TaskListCompleted(vacancy);

            actual.Should().BeTrue();
        }
    }
}