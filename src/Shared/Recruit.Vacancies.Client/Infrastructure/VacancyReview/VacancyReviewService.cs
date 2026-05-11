using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Responses;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;

public class VacancyReviewService(IOuterApiClient outerApiClient,
    IEncodingService encodingService,
    IFeature feature)
    : IVacancyReviewRepository, IVacancyReviewQuery
{
    public async Task CreateAsync(Domain.Entities.VacancyReview vacancyReview)
    {
        var vacancyReviewDto = VacancyReviewDto.MapVacancyReviewDto(vacancyReview, encodingService, feature.IsFeatureEnabled(FeatureNames.QaAi));
        await outerApiClient.Post(new PostVacancyReviewRequest(vacancyReview.Id, vacancyReviewDto));
    }

    public async Task<Domain.Entities.VacancyReview> GetAsync(Guid reviewId)
    {
        var result = await outerApiClient.Get<GetVacancyReviewResponse>(new GetVacancyReviewRequest(reviewId));

        if (result == null)
        {
            return null;
        }
        
        return (Domain.Entities.VacancyReview)result.VacancyReview;
    }

    public async Task UpdateAsync(Domain.Entities.VacancyReview review)
    {
        await outerApiClient.Post(new PostVacancyReviewRequest(review.Id,VacancyReviewDto.MapVacancyReviewDto(review, encodingService)), false);
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetForVacancyAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<Domain.Entities.VacancyReview> GetLatestReviewByReferenceAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference));
        
        if (result == null)
        {
            return null;
        }

        var filtered = result.VacancyReviews
            .Where(r =>
                r.VacancyReference == vacancyReference &&
                (
                    r.ManualOutcome == null ||
                    (
                        r.ManualOutcome != ManualQaOutcome.Transferred.ToString() &&
                        r.ManualOutcome != ManualQaOutcome.Blocked.ToString()
                    )
                )
            )
            .OrderByDescending(r => r.CreatedDate)
            .FirstOrDefault();

        return (Domain.Entities.VacancyReview)filtered;
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetByStatusAsync(ReviewStatus status)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByFilterRequest([status]));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<Domain.Entities.VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, ReviewStatus.Closed));
        
        if (result == null)
        {
            return null;
        }

        var filtered = result.VacancyReviews
            .Where(r =>
                r.Status == ReviewStatus.Closed.ToString() &&
                r.ManualOutcome == ManualQaOutcome.Referred.ToString())
            .OrderByDescending(r => r.ClosedDate)
            .FirstOrDefault();

        return (Domain.Entities.VacancyReview)filtered;
    }
}