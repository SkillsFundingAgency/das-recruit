using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using static Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests.GetApplicationReviewsByIdsApiRequest;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
public record GetApplicationReviewsByIdsApiRequest(GetApplicationReviewsByIdsApiRequestData Payload) : IPostApiRequest
{
    public string PostUrl => "applicationReviews/manyByApplicationIds";
    public object Data { get; set; } = Payload;

    public record GetApplicationReviewsByIdsApiRequestData
    {
        public List<Guid> ApplicationReviewIds { get; set; } = [];
    }
}