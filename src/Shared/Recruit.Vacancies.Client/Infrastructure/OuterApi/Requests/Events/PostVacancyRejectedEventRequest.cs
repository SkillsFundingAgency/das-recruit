using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostVacancyRejectedEventData(Guid VacancyId);

public class PostVacancyRejectedEventRequest(PostVacancyRejectedEventData data): IPostApiRequest
{
    public string PostUrl => "events/vacancy-rejected";
    public object Data { get; set; } = data;
}