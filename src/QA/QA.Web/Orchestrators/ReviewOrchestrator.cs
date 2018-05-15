using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Exceptions;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class ReviewOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;
        private readonly ReviewMapper _mapper;

        public ReviewOrchestrator(IQaVacancyClient vacancyClient, ReviewMapper mapper)
        {
            _vacancyClient = vacancyClient;
            _mapper = mapper;
        }

        public Task ApproveReviewAsync(Guid reviewId)
        {
            return _vacancyClient.ApproveReview(reviewId);
        }

        public async Task ApproveReferredReviewAsync(Guid reviewId, ReferralViewModel reviewChanges)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);

            _mapper.MapChangesOntoVacancy(vacancy, reviewChanges);

            await _vacancyClient.ApproveReferredReviewAsync(reviewId, vacancy);
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(Guid reviewId, VacancyUser user)
        {            
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {reviewId}");

            if (review.Status != ReviewStatus.PendingReview && review.Status != ReviewStatus.UnderReview)
            {
                throw new InvalidStateException($"Review is not in a correct state. State: {review.Status}");
            }

            if (review.Status == ReviewStatus.PendingReview)
            {
                await _vacancyClient.StartReview(review.Id, user);
            }

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
            var vm = await _mapper.MapFromVacancy(vacancy);
            
            return vm;
        }

        public async Task<ReviewViewModel> GetReferralViewModelAsync(Guid reviewId)
        {            
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {reviewId}");

            if (review.ManualOutcome != ManualQaOutcome.Referred)
            {
                await _vacancyClient.ReferVacancyReviewAsync(review.Id);
            }

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
            var vm = await _mapper.MapFromVacancy(vacancy);
            vm.IsEditable = true;
            
            return vm;
        }
    }
}