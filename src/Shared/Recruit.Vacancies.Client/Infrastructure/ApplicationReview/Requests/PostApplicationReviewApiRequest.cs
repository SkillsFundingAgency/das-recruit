using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests
{
    public record PostApplicationReviewApiRequest(Guid ApplicationReviewId, PostApplicationReviewApiRequestData Payload) : IPostApiRequest
    {
        public string PostUrl
        {
            get
            {
                return $"applicationReviews/{ApplicationReviewId}";
            }
        }

        public object Data { get; set; } = Payload;
    }

    public record PostApplicationReviewApiRequestData
    {
        public bool HasEverBeenEmployerInterviewing { get; init; }
        public DateTime? DateSharedWithEmployer { get; init; }
        public string? EmployerFeedback { get; init; } = null;
        public required string Status { get; init; }
        public string? TemporaryReviewStatus { get; init; } = null;
    }
}