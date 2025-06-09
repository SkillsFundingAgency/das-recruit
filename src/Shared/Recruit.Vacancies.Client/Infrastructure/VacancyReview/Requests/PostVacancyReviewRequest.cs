using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class PostVacancyReviewRequest(Guid id, VacancyReviewDto vacancyReview) : IPostApiRequest
{
    public string PostUrl => $"VacancyReviews/{id}";
    public object Data { get; set; } = vacancyReview;
}