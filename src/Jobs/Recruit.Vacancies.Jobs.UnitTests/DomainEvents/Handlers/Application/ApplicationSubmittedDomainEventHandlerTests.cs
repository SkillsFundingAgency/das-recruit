using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers.Application;

public class ApplicationSubmittedDomainEventHandlerTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Event_Is_Handled(
        ApplicationSubmittedEvent sourceEvent,
        [Frozen] Mock<IJobsVacancyClient> mockJobsClient,
        ApplicationSubmittedDomainEventHandler sut)
    {
        // Arrange
        var eventPayload = JsonConvert.SerializeObject(sourceEvent);
        mockJobsClient.Setup(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await sut.HandleAsync(eventPayload);

        // Assert
        mockJobsClient.Verify(x => x.CreateApplicationReviewAsync(It.IsAny<Esfa.Recruit.Vacancies.Client.Domain.Entities.Application>()), Times.Once);
    }
}