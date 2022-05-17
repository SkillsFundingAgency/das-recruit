using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class CreateProviderOwnedVacancyCommandHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_Command_Is_Handled_With_Vacancy_Saved_And_Message_Published(
            DateTime createdDate,
            CreateProviderOwnedVacancyCommand command,
            [Frozen] Mock<ITimeProvider> timeProvider,
            [Frozen] Mock<IMessaging> messaging,
            [Frozen] Mock<IVacancyRepository> vacancyRepository)
        {
            //Arrange
            var serviceParameters = new ServiceParameters("Apprenticeship");
            timeProvider.Setup(x => x.Now).Returns(createdDate);
            var handler = new CreateProviderOwnedVacancyCommandHandler(
                Mock.Of<ILogger<CreateProviderOwnedVacancyCommandHandler>>(), 
                vacancyRepository.Object, 
                messaging.Object, 
                timeProvider.Object, 
                serviceParameters);
            
            //Act
            await handler.Handle(command, CancellationToken.None);
            
            //Assert
            vacancyRepository.Verify(x=>x.CreateAsync(
                It.Is<Vacancy>(
                    c=>
                        c.Id.Equals(command.VacancyId)
                        && c.SourceOrigin.Equals(command.Origin)
                        && c.SourceType.Equals(SourceType.New)
                        && c.EmployerAccountId.Equals(command.EmployerAccountId)
                        && c.TrainingProvider.Ukprn.Equals(command.Ukprn)
                        && c.Status.Equals(VacancyStatus.Draft)
                        && c.CreatedDate.Equals(createdDate)
                        && c.CreatedByUser.Equals(command.User)
                        && c.LastUpdatedDate.Equals(createdDate)
                        && c.LastUpdatedByUser.Equals(command.User)
                        && !c.IsDeleted
                        && c.Title.Equals(command.Title)
                        && c.VacancyType.Equals(VacancyType.Apprenticeship)
                        && c.ApplicationMethod == null
                )), Times.Once);
            messaging.Verify(x=>x.PublishEvent(
                It.Is<VacancyCreatedEvent>(c=>c.VacancyId.Equals(command.VacancyId))));
            
        }

        [Test, MoqAutoData]
        public async Task Then_If_Traineeship_Type_Vacancy_ApplicationMethod_Set(
            DateTime createdDate,
            CreateProviderOwnedVacancyCommand command,
            [Frozen] Mock<ITimeProvider> timeProvider,
            [Frozen] Mock<IMessaging> messaging,
            [Frozen] Mock<IVacancyRepository> vacancyRepository)
        {
            //Arrange
            var serviceParameters = new ServiceParameters("Traineeship");
            timeProvider.Setup(x => x.Now).Returns(createdDate);
            var handler = new CreateProviderOwnedVacancyCommandHandler(
                Mock.Of<ILogger<CreateProviderOwnedVacancyCommandHandler>>(), 
                vacancyRepository.Object, 
                messaging.Object, 
                timeProvider.Object, 
                serviceParameters);
            
            //Act
            await handler.Handle(command, CancellationToken.None);
            
            //Assert
            vacancyRepository.Verify(x=>x.CreateAsync(
                It.Is<Vacancy>(
                    c=>
                        c.Id.Equals(command.VacancyId)
                        && c.VacancyType.Equals(VacancyType.Traineeship)
                        && c.ApplicationMethod.Equals(ApplicationMethod.ThroughFindATraineeship)
                )), Times.Once);
            messaging.Verify(x=>x.PublishEvent(
                It.Is<VacancyCreatedEvent>(c=>c.VacancyId.Equals(command.VacancyId))));
        }
    }
}