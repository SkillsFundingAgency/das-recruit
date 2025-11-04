using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;

public record PostApplicationSharedNotificationApiRequest(
    PostApplicationSharedNotificationApiRequest.PostApplicationSharedNotificationApiRequestData Payload)
    : IPostApiRequest
{
    public string PostUrl => "events/application-shared-with-employer";

    public object Data { get; set; } = Payload;

    public record PostApplicationSharedNotificationApiRequestData
    {
        public required string HashAccountId { get; init; }
        public required long AccountId { get; init; }
        public required Guid VacancyId { get; init; }
        public required Guid ApplicationId { get; init; }
        public required string TrainingProvider { get; init; }
        public required string AdvertTitle { get; init; }
        public required long VacancyReference { get; init; }
    }
}
