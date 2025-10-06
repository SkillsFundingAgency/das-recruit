using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsCountByVacancyReferenceApiRequest(long VacancyReference) : IGetApiRequest
{
    public string GetUrl => $"applicationReviews/vacancyReference/{VacancyReference}/count";
}
