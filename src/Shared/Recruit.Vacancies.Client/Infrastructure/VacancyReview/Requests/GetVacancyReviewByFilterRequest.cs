using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class GetVacancyReviewByFilterRequest(List<ReviewStatus>? status = null, DateTime? expiredAssignationDateTime = null) : IGetApiRequest
{
    public string GetUrl
    {
        get
        {
            return status != null 
                ? $"VacancyReviews?reviewStatus={string.Join("&reviewStatus=", status)}&expiredAssignationDateTime={expiredAssignationDateTime}" 
                : $"VacancyReviews?reviewStatus=&expiredAssignationDateTime={expiredAssignationDateTime}";
        }
    }
}