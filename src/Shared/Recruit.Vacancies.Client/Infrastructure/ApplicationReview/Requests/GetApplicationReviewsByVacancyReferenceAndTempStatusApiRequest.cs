using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsByVacancyReferenceAndTempStatusApiRequest(long VacancyReference,
    ApplicationReviewStatus Status) : IGetApiRequest
{
    public string GetUrl => $"applicationReviews/vacancyReference/{VacancyReference}/temp-status/{Status}";
}
