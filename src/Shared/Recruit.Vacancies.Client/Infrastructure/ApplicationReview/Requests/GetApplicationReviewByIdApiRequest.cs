using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewByIdApiRequest(Guid ApplicationReviewId) : IGetApiRequest
{
    public string GetUrl
    {
        get
        {
            return $"applicationReviews/{ApplicationReviewId}";
        }
    }
}