using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostApplicationSubmittedEventData(Guid ApplicationId, long VacancyReference);

public class PostApplicationSubmittedEventRequest(PostApplicationSubmittedEventData data) : IPostApiRequest
{
    public string PostUrl => "events/application-submitted";
    public object Data { get; set; } = data;
}
