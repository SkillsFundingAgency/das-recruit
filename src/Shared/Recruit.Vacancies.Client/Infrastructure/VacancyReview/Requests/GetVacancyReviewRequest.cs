using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class GetVacancyReviewRequest(Guid id) : IGetApiRequest
{
    public string GetUrl => $"VacancyReviews/{id}";
}