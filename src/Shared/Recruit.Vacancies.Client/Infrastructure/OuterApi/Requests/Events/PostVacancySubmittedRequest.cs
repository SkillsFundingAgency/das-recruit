using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostVacancySubmittedEventData(Guid VacancyId, long VacancyReference);

public class PostVacancySubmittedEventRequest(PostVacancySubmittedEventData data): IPostApiRequest
{
    public string PostUrl => "events/vacancy-submitted";
    public object Data { get; set; } = data;
}