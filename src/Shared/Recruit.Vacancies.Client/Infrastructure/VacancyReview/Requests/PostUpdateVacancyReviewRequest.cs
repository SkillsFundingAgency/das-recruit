using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class PostUpdateVacancyReviewRequest(VacancyReviewDto vacancyReview) : IPostApiRequest
{
    public string PostUrl => $"VacancyReviews/{vacancyReview.Id}/update";
    public object Data { get; set; } = vacancyReview;
}