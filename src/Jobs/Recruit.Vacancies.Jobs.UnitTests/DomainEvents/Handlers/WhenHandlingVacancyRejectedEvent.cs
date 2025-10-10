using System.Text.Json;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using NUnit.Framework;

namespace Recruit.Vacancies.Jobs.UnitTests.DomainEvents.Handlers;

public class WhenHandlingVacancyRejectedEvent
{
    [Test, MoqAutoData]
    public async Task Then_The_Event_Is_Passed_To_Apim(
        VacancyRejectedEvent sourceEvent,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] VacancyRejectedHandler sut)
    {
        // arrange
        string @event = JsonSerializer.Serialize(sourceEvent);

        PostVacancyRejectedEventRequest? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Post(It.IsAny<PostVacancyRejectedEventRequest>(), true))
            .Callback<IPostApiRequest, bool>((x, _) => capturedRequest = x as PostVacancyRejectedEventRequest);

        // act
        await sut.HandleAsync(@event);

        //Assert
        outerApiClient.Verify(x => x.Post(It.IsAny<PostVacancyRejectedEventRequest>(), true), Times.Once);
        capturedRequest.PostUrl.Should().Be("events/vacancy-rejected");
        var data = capturedRequest.Data as PostVacancyRejectedEventData;
        data.Should().NotBeNull();
        data!.VacancyId.Should().Be(sourceEvent.VacancyId);
    }
}