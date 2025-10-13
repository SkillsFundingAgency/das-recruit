using System.Linq;
using System.Text.Json;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using NUnit.Framework;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers;

public class WhenHandlingReviewVacancyEvent
{
    [Test, MoqAutoData]
    public async Task GivenVacancyReviewedEvent_VerifyCommunicationRequestIsSentToCommunicationQueue(
        VacancyReviewedEvent sourceEvent,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ICommunicationQueueService> commsService,
        [Greedy] VacancyReviewedHandler sut)
    {
        // arrange
        var @event = JsonSerializer.Serialize(sourceEvent);

        feature
            .Setup(x => x.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            .Returns(false);

        // act
        await sut.HandleAsync(@event);

        // assert
        commsService.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Once);
        outerApiClient.Verify(x => x.Post(It.IsAny<IPostApiRequest>(), It.IsAny<bool>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task GivenVacancyReviewedEvent_VerifyCommunicationRequestHasExpectedData(
        VacancyReviewedEvent sourceEvent,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ICommunicationQueueService> commsService,
        [Greedy] VacancyReviewedHandler sut)
    {
        // arrange
        var @event = JsonSerializer.Serialize(sourceEvent);
            
        CommunicationRequest? capturedRequest = null;
        commsService.Setup(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()))
            .Returns(Task.CompletedTask)
            .Callback<CommunicationRequest>(x => capturedRequest = x);

        feature
            .Setup(x => x.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            .Returns(false);

        // act
        await sut.HandleAsync(@event);

        // assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Entities
            .Any(cr => cr.EntityType.Equals(CommunicationConstants.EntityTypes.Vacancy) && ((long)cr.EntityId == sourceEvent.VacancyReference))
            .Should().BeTrue();
        outerApiClient.Verify(x => x.Post(It.IsAny<IPostApiRequest>(), It.IsAny<bool>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_New_Notifications_Are_Enabled_The_Outer_Api_Is_Called(
        VacancyReviewedEvent sourceEvent,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ICommunicationQueueService> commsService,
        [Greedy] VacancyReviewedHandler sut)
    {
        // arrange
        var @event = JsonSerializer.Serialize(sourceEvent);
        feature
            .Setup(x => x.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            .Returns(true);

        PostVacancySubmittedEventRequest? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Post(It.IsAny<IPostApiRequest>(), It.IsAny<bool>()))
            .Callback<IPostApiRequest, bool>((x, _) => capturedRequest = x as PostVacancySubmittedEventRequest);

        // act
        await sut.HandleAsync(@event);

        // assert
        capturedRequest.Should().NotBeNull();
        var data = capturedRequest.Data as PostVacancySubmittedEventData;
        data!.VacancyId.Should().Be(sourceEvent.VacancyId);
            
        commsService.Verify(x => x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Never);
        outerApiClient.Verify(x => x.Post(It.IsAny<IPostApiRequest>(), It.IsAny<bool>()), Times.Once);
    }
}