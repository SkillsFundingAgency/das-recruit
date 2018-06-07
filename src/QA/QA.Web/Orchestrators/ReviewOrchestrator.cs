using System;
using System.Linq;
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
            await _vacancyClient.ApproveReferredReviewAsync(
                reviewId, 
                reviewChanges.ShortDescription,
                reviewChanges.VacancyDescription,
                reviewChanges.TrainingDescription,
                reviewChanges.OutcomeDescription,
                reviewChanges.ThingsToConsider,
                reviewChanges.EmployerDescription);
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(Guid reviewId, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            ValidateReviewStateForViewing(review);

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
            
            ValidateVacancyStateForViewing(review, vacancy);

            if (review.Status == ReviewStatus.PendingReview)
            {
                await _vacancyClient.StartReview(review.Id, user);
            }

            var vm = await _mapper.MapFromVacancy(vacancy);

            return vm;
        }

        public async Task<ReviewViewModel> GetReferralViewModelAsync(Guid reviewId)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            ValidateReviewStateForReferral(review);

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
            
            ValidateVacancyForReferral(review, vacancy);

            if (review.ManualOutcome != ManualQaOutcome.Referred)
            {
                await _vacancyClient.ReferVacancyReviewAsync(review.Id);
            }

            var vm = await _mapper.MapFromVacancy(vacancy);
            vm.IsEditable = true;

            return vm;
        }

        private void ValidateVacancyForReferral(VacancyReview review, Vacancy vacancy)
        {
            if (vacancy == null)
            {
                throw new NotFoundException($"Unable to find vacancy with reference: {review.VacancyReference}");
            }

            if (!IsValidStateForReferral(vacancy.Status))
            {
                throw new InvalidStateException($"Vacancy is not in a correct state for referral view. State: {vacancy.Status}");
            }
        }

        private static void ValidateReviewStateForReferral(VacancyReview review)
        {
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {review.Id}");

            if (review.Status != ReviewStatus.UnderReview)
            {
                throw new InvalidStateException($"Review is not in a correct state for referring. State: {review.Status}");
            }
        }

        private void ValidateVacancyStateForViewing(VacancyReview review, Vacancy vacancy)
        {
            if (vacancy == null)
            {
                throw new NotFoundException($"Unable to find vacancy with reference: {review.VacancyReference}");
            }

            if (!IsValidStateForViewing(vacancy.Status))
            {
                throw new InvalidStateException($"Vacancy is not in a correct state for viewing. State: {vacancy.Status}");
            }
        }

        private static void ValidateReviewStateForViewing(VacancyReview review)
        {
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {review.Id}");

            if (review.Status != ReviewStatus.PendingReview && review.Status != ReviewStatus.UnderReview)
            {
                throw new InvalidStateException($"Review is not in a correct state for viewing. State: {review.Status}");
            }
        }

        private bool IsValidStateForViewing(VacancyStatus status)
        {
            var validStatuses = new VacancyStatus[] 
            {
                VacancyStatus.PendingReview
            };

            return validStatuses.Contains(status);
        }

        private bool IsValidStateForReferral(VacancyStatus status)
        {
            var validStatuses = new VacancyStatus[] 
            {
                VacancyStatus.PendingReview,
                VacancyStatus.UnderReview
            };

            return validStatuses.Contains(status);
        }
    }
}