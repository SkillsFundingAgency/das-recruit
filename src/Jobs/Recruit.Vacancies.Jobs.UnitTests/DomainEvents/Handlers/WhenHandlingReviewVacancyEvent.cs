using System.Text.Json;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using NUnit.Framework;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers;

public class WhenHandlingReviewVacancyEvent
{
    [Test, MoqAutoData]
    public async Task Then_If_New_Notifications_Are_Enabled_The_Outer_Api_Is_Called(
        VacancyReviewedEvent sourceEvent,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] VacancyReviewedHandler sut)
    {
        // arrange
        var @event = JsonSerializer.Serialize(sourceEvent);
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
        outerApiClient.Verify(x => x.Post(It.IsAny<IPostApiRequest>(), It.IsAny<bool>()), Times.Once);
    }
}