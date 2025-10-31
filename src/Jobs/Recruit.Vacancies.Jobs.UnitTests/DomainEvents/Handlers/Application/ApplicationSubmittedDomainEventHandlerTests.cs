using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Application;

public class ApplicationSubmittedDomainEventHandlerTests
{

    [Test, MoqAutoData]
    public async Task  When_NotificationsMigration_Disabled_CommunicationRequestIsSent_And_ApplicationReviewCreated(
        ApplicationSubmittedEvent sourceEvent,
        [Frozen] Mock<ICommunicationQueueService> mockCommQueue,
        [Frozen] Mock<IJobsVacancyClient> mockJobsClient,
        [Frozen] Mock<IFeature> mockFeature,
        ApplicationSubmittedDomainEventHandler sut)
    {
        // Arrange
        var eventPayload = JsonConvert.SerializeObject(sourceEvent);
        CommunicationRequest sentRequest = null;
        mockCommQueue.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
            .Returns(Task.CompletedTask)
            .Callback<CommunicationRequest>(cr => sentRequest = cr);
        mockJobsClient.Setup(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockFeature.Setup(f => f.IsFeatureEnabled(It.Is<string>(s => s == FeatureNames.NotificationsMigration))).Returns(false);

        // Act
        await sut.HandleAsync(eventPayload);

        // Assert
        mockCommQueue.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Once);
        sentRequest.Should().NotBeNull();
        sentRequest.Entities.Should().Contain(e => e.EntityType.Equals(CommunicationConstants.EntityTypes.Vacancy) && (long)e.EntityId == sourceEvent.Application.VacancyReference);
        mockJobsClient.Verify(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task When_NotificationsMigration_Enabled_Then_The_CommunicationRequest_Is_Not_Created(
        ApplicationSubmittedEvent sourceEvent,
        [Frozen] Mock<ICommunicationQueueService> mockCommQueue,
        [Frozen] Mock<IJobsVacancyClient> mockJobsClient,
        [Frozen] Mock<IFeature> mockFeature,
        ApplicationSubmittedDomainEventHandler sut)
    {
        // Arrange
        var eventPayload = JsonConvert.SerializeObject(sourceEvent);
        mockCommQueue.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
            .Returns(Task.CompletedTask);
        mockJobsClient.Setup(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockFeature.Setup(f => f.IsFeatureEnabled(It.Is<string>(s => s == FeatureNames.NotificationsMigration))).Returns(true);

        // Act
        await sut.HandleAsync(eventPayload);

        // Assert
        mockJobsClient.Verify(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()), Times.Once);
        mockCommQueue.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Never);
    }
}