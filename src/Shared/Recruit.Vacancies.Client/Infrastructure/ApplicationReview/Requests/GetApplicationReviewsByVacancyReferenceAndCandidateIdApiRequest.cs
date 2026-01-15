using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsByVacancyReferenceAndCandidateIdApiRequest(long VacancyReference,
    Guid CandidateId) : IGetApiRequest
{
    public string GetUrl => $"applicationReviews/{VacancyReference}/candidate/{CandidateId}";
}