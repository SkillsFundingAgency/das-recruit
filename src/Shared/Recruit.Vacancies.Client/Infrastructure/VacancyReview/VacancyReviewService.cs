using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Responses;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;


public interface IVacancyReviewRepositoryRunner
{
    Task CreateAsync(Domain.Entities.VacancyReview vacancy);
    Task UpdateAsync(Domain.Entities.VacancyReview review);
}

public class VacancyReviewRepositoryRunner : IVacancyReviewRepositoryRunner
{
    private readonly IEnumerable<IVacancyReviewRepository> _vacancyReviewResolver;

    public VacancyReviewRepositoryRunner(IEnumerable<IVacancyReviewRepository> vacancyReviewResolver)
    {
        _vacancyReviewResolver = vacancyReviewResolver;
    }

    public async Task UpdateAsync(Domain.Entities.VacancyReview vacancyReview)
    {
        foreach (var vacancyReviewResolver in _vacancyReviewResolver)
        {
            await vacancyReviewResolver.UpdateAsync(vacancyReview);
        }
    }
    public async Task CreateAsync(Domain.Entities.VacancyReview vacancyReview)
    {
        foreach (var vacancyReviewResolver in _vacancyReviewResolver)
        {
            await vacancyReviewResolver.CreateAsync(vacancyReview);
        }
    }
}

public class VacancyReviewService(IOuterApiClient outerApiClient, IEncodingService encodingService) : IVacancyReviewRepository, IVacancyReviewQuery
{
    public string Key { get; } = "OuterApiVacancyReview";
    public async Task CreateAsync(Domain.Entities.VacancyReview vacancy)
    {
        await outerApiClient.Post(new PostVacancyReviewRequest(vacancy.Id, VacancyReviewDto.MapVacancyReviewDto(vacancy, encodingService)), false);
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

    public Task<List<VacancyReviewSummary>> GetActiveAsync()
    {
        //NOTE: WILL NOT IMPLEMENT. To implement as a more efficient query in GetVacancyReviewByFilterRequest
        throw new NotImplementedException();
    }

    public async Task<GetVacancyReviewSummaryResponse> GetVacancyReviewSummary()
    {
        return await outerApiClient.Get<GetVacancyReviewSummaryResponse>(new GetVacancyReviewSummaryRequest());
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetForVacancyAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<Domain.Entities.VacancyReview> GetLatestReviewByReferenceAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, "latest"));
        
        if (result == null)
        {
            return null;
        }
        
        return (Domain.Entities.VacancyReview)result.VacancyReviews.FirstOrDefault();
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetByStatusAsync(ReviewStatus status)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByFilterRequest([status]));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetVacancyReviewsInProgressAsync(DateTime getExpiredAssignationDateTime)
    {
        var result = await outerApiClient.Get<GetVacancyReviewListResponse>(new GetVacancyReviewByFilterRequest(expiredAssignationDateTime:getExpiredAssignationDateTime));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<int> GetApprovedCountAsync(string submittedByUserId)
    {
        var result = await outerApiClient.Get<GetVacancyReviewCountResponse>(new GetVacancyReviewCountByUserFilterRequest(submittedByUserId));
        return result.Count;
    }

    public async Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId)
    {
        // GETVacancyReviewCountByAccountLegalEntityPublicHashedId
        // where status closed
        // ManualOutcome approved
        // EmployerNameOption anonymous
        var result = await outerApiClient.Get<GetVacancyReviewCountResponse>(new GetVacancyReviewCountByUserFilterRequest(submittedByUserId, true));
        return result.Count;
    }

    public async Task<List<Domain.Entities.VacancyReview>> GetAssignedForUserAsync(string userId, DateTime assignationExpiryDateTime)
    {
        var result =
            await outerApiClient.Get<GetVacancyReviewListResponse>(
                new GetVacancyReviewsAssignedToUserRequest(userId, assignationExpiryDateTime));
        return result.VacancyReviews.Select(c=>(Domain.Entities.VacancyReview)c).ToList();
    }

    public async Task<Domain.Entities.VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference)
    {
        var result = await outerApiClient.Get<GetVacancyReviewResponse>(new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, "latestReferred"));
        
        if (result == null)
        {
            return null;
        }
        
        return (Domain.Entities.VacancyReview)result.VacancyReview;
    }

    public async Task<int> GetAnonymousApprovedCountAsync(string accountLegalEntityPublicHashedId)
    {
        var accountLegalEntity =
            encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        // this is just used as a flag so can just return 1 or zero
        var result = await outerApiClient.Get<GetVacancyReviewCountResponse>(new GetAnonymousApprovedCountByAccountLegalEntity(accountLegalEntity));
        return result.Count;
    }
}