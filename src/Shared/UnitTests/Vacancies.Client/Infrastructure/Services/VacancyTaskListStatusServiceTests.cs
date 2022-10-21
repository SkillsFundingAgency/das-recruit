using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
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
        public void When_Null_VacancyType_And_Has_ApplicationMethod_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = null;
            vacancy.Object.ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public void When_Traineeship_And_Not_Viewed_Provider_Contact_Details_Is_Null_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Traineeship;
            vacancy.Object.HasChosenProviderContactDetails = null;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }
        [Test, MoqAutoData]
        public void When_Traineeship_And_Not_Viewed_Provider_Contact_Details_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Traineeship;
            vacancy.Object.HasChosenProviderContactDetails = false;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void When_Traineeship_And_Has_ViewedProviderContactDetails_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.VacancyType = VacancyType.Traineeship;
            vacancy.Object.HasChosenProviderContactDetails = true;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
    }
}