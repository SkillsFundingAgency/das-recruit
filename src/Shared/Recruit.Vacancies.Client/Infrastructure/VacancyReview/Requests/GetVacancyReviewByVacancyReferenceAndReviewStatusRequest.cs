using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(long vacancyReference, ReviewStatus? reviewStatus = null) : IGetApiRequest
{
    public string GetUrl => $"vacancies/{vacancyReference}/vacancyReviews?status={reviewStatus}";
}