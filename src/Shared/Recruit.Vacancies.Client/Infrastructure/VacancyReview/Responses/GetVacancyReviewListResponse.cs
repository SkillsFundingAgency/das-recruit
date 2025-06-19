using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Responses;

public class GetVacancyReviewListResponse
{
    public List<VacancyReviewDto> VacancyReviews { get; set; }
}