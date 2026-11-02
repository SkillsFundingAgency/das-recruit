using System;
using Esfa.Recruit.Vacancies.Client.Application;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostLiveVacancyUpdatedEventData(Guid VacancyId, long VacancyReference, LiveUpdateKind UpdateKind);

public class PostLiveVacancyUpdatedEventRequest(PostLiveVacancyUpdatedEventData data): IPostApiRequest
{
    public string PostUrl => "events/live-vacancy-updated";
    public object Data { get; set; } = data;
}