using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostEmployerRejectedVacancyEventData(Guid VacancyId);

public class PostEmployerRejectedVacancyEventRequest(PostEmployerRejectedVacancyEventData data): IPostApiRequest
{
    public string PostUrl => "events/employer-rejected-vacancy";
    public object Data { get; set; } = data;
}