namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
public record GetApplicationReviewByVacancyReferenceAndCandidateIdApiResponse
{
    public ApplicationReview? ApplicationReview { get; init; }
}