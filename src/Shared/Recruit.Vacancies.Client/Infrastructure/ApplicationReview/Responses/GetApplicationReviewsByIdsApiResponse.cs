using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses
{
    public record GetApplicationReviewsByIdsApiResponse
    {
        public List<ApplicationReview> ApplicationReviews { get; init; } = [];
    }
}