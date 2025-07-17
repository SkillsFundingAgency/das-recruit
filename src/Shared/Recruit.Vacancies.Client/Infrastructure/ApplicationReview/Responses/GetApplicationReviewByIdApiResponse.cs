namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
public record GetApplicationReviewByIdApiResponse
{
    public ApplicationReview ApplicationReview { get; init; } = new();
}
