using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyTaskListStatusServiceTests
    {
        [Test, MoqAutoData]
        public void When_Apprenticeship_And_Has_ApplicationMethod_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Apprenticeship;
            vacancy.Object.ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public void When_Apprenticeship_And_No_ApplicationMethod_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Apprenticeship;
            vacancy.Object.ApplicationMethod = null;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void When_Traineeship_And_No_EmployerDescription_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Traineeship;
            vacancy.Object.EmployerDescription = null;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void When_Traineeship_And_Has_EmployerDescription_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Traineeship;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
    }

    public interface ITaskListVacancy
    {
        public string EmployerDescription { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public VacancyType? VacancyType { get; set; }
    }

    public class VacancyTaskListStatusService
    {
        public bool IsTaskListCompleted(ITaskListVacancy vacancy)
        {
            if(vacancy.VacancyType == VacancyType.Apprenticeship)
                return vacancy.ApplicationMethod != null;
            if (vacancy.VacancyType == VacancyType.Traineeship)
                return !string.IsNullOrEmpty(vacancy.EmployerDescription);

            return false;
        }
    }
}