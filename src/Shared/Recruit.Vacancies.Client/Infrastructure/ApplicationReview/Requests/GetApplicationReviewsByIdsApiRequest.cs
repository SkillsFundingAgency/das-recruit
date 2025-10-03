using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsByIdsApiRequest(List<Guid> ApplicationIds) : IPostApiRequest
{
    public string PostUrl => "applicationReviews";
    public object Data { get; set; } = ApplicationIds;
}