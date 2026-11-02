#nullable enable
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;

public record GetApplicationReviewsByVacancyReferenceApiResponse
{
    public List<ApplicationReview> ApplicationReviews { get; init; } = [];
}