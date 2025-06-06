using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class PostVacancyReviewRequest(VacancyReviewDto vacancyReview) : IPostApiRequest
{
    public string PostUrl => "VacancyReviews";
    public object Data { get; set; } = vacancyReview;
}