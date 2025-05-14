﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;
using OwnerType = Esfa.Recruit.Vacancies.Client.Domain.Entities.OwnerType;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyTaskListStatusServiceTests
    {
        [Test, MoqAutoData]
        public void When_Employer_Has_Submitted_Additional_Questions_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Employer;
            vacancy.Object.HasSubmittedAdditionalQuestions = true;

            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public void When_Employer_And_No_AdditionalQuestions_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Employer;
            vacancy.Object.HasSubmittedAdditionalQuestions = false;

            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public void When_Null_VacancyType_And_Employer_OwnerType_And_Has_AdditionalQuestions_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Employer;
            vacancy.Object.HasSubmittedAdditionalQuestions = true;

            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public void When_Provider_And_Apprenticeship_And_HasSubmittedAdditionalQuestions_True_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Provider;
            vacancy.Object.HasSubmittedAdditionalQuestions = true;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public void When_Provider_And_Apprenticeship_And_HasSubmittedAdditionalQuestions_False_Then_TaskList_Not_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Provider;
            vacancy.Object.HasSubmittedAdditionalQuestions = false;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeFalse();
        }
        
        [Test, MoqAutoData]
        public void When_Null_VacancyType_And_Provider_Ownertype_And_HasSubmittedAdditionalQuestions_True_Then_TaskList_Completed(VacancyTaskListStatusService service, Mock<ITaskListVacancy> vacancy)
        {
            vacancy.Object.OwnerType = OwnerType.Provider;
            vacancy.Object.HasSubmittedAdditionalQuestions = true;
            
            bool result = service.IsTaskListCompleted(vacancy.Object);

            result.Should().BeTrue();
        }
    }
}