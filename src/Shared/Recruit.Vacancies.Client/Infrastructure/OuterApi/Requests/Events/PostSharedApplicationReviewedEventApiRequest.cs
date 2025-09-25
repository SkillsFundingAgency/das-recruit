using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostSharedApplicationReviewedEventApiRequest(
    PostSharedApplicationReviewedEventApiRequest.PostSharedApplicationReviewedEventApiRequestData Payload)
    : IPostApiRequest
{
    public string PostUrl => "events/shared-application-reviewed";
    public object Data { get; set; } = Payload;

    public record PostSharedApplicationReviewedEventApiRequestData(Guid VacancyId, long VacancyReference);
}