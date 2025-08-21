using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
public record PostApplicationSharedNotificationApiRequest(
    PostApplicationSharedNotificationApiRequest.PostApplicationSharedNotificationApiRequestData Payload)
    : IPostApiRequest
{
    public string PostUrl => "applicationReviews/notification/shared-with-employer";

    public object Data { get; set; } = Payload;

    public record PostApplicationSharedNotificationApiRequestData
    {
        public required string HashAccountId { get; init; }
        public required Guid VacancyId { get; init; }
        public required Guid ApplicationId { get; init; }
        public required string RecipientEmail { get; init; }
        public required string FirstName { get; init; }
        public required string TrainingProvider { get; init; }
        public required string AdvertTitle { get; init; }
        public required long VacancyReference { get; init; }
    }
}
