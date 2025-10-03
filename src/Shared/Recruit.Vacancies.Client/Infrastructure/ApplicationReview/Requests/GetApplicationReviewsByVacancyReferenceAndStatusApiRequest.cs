using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsByVacancyReferenceAndStatusApiRequest(long VacancyReference,
    ApplicationReviewStatus Status,
    bool IncludeTemporaryStatus = false) : IGetApiRequest
{
    public string GetUrl => $"applicationReviews/{VacancyReference}/status/{Status}?includeTemporaryStatus={IncludeTemporaryStatus}";
}
