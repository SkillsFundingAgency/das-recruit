using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(long vacancyReference, string reviewStatus = null) : IGetApiRequest
{
    public string GetUrl => $"{vacancyReference}/VacancyReviews?status={reviewStatus}";
}