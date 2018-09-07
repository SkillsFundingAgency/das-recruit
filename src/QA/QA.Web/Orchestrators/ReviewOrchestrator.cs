using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Exceptions;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using UnassignedVacancyReviewException = Esfa.Recruit.Qa.Web.Exceptions.UnassignedVacancyReviewException;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class ReviewOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;
        private readonly ReviewMapper _mapper;

        private static readonly List<string> FieldIndicators = new List<string>
        {
            VacancyReview.FieldIdentifiers.ApplicationProcess,
            VacancyReview.FieldIdentifiers.ClosingDate,
            VacancyReview.FieldIdentifiers.Contact,
            VacancyReview.FieldIdentifiers.Description,
            VacancyReview.FieldIdentifiers.DisabilityConfident,
            VacancyReview.FieldIdentifiers.EmployerAddress,
            VacancyReview.FieldIdentifiers.EmployerName,
            VacancyReview.FieldIdentifiers.EmployerDescription,
            VacancyReview.FieldIdentifiers.EmployerWebsiteUrl,
            VacancyReview.FieldIdentifiers.ExpectedDuration,
            VacancyReview.FieldIdentifiers.NumberOfPositions,
            VacancyReview.FieldIdentifiers.PossibleStartDate,
            VacancyReview.FieldIdentifiers.Provider,
            VacancyReview.FieldIdentifiers.Qualifications,
            VacancyReview.FieldIdentifiers.Skills,
            VacancyReview.FieldIdentifiers.ShortDescription,
            VacancyReview.FieldIdentifiers.ThingsToConsider,
            VacancyReview.FieldIdentifiers.Training,
            VacancyReview.FieldIdentifiers.TrainingLevel,
            VacancyReview.FieldIdentifiers.Wage,
            VacancyReview.FieldIdentifiers.WorkingWeek,
        };

        public ReviewOrchestrator(IQaVacancyClient vacancyClient, ReviewMapper mapper)
        {
            _vacancyClient = vacancyClient;
            _mapper = mapper;
        }

        public async Task<Guid?> SubmitReviewAsync(ReviewEditModel m, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(m.ReviewId);
            await EnsureUserIsAssignedAsync(review, user.UserId);

            var manualQaFieldIndicators = FieldIndicators.Select(f => new ManualQaFieldIndicator
            {
                FieldIdentifier = f,
                IsChangeRequested = m.SelectedFieldIdentifers.Contains(f)
            }).ToList();

            if (m.IsRefer)
            {
                await _vacancyClient.ReferVacancyReviewAsync(m.ReviewId, m.ReviewerComment, manualQaFieldIndicators);
            }
            else
            {
                await _vacancyClient.ApproveVacancyReviewAsync(m.ReviewId, m.ReviewerComment, manualQaFieldIndicators);
            }

            var nextVacancyReviewId = await AssignNextVacancyReviewAsync(user);

            return nextVacancyReviewId;
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

            if (_vacancyClient.VacancyReviewCanBeAssigned(review))
            {
                await _vacancyClient.AssignVacancyReviewAsync(user, review.Id);
                review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            }

            await EnsureUserIsAssignedAsync(review, user.UserId);

            var vm = await _mapper.MapFromVacancy(review.VacancySnapshot);

            return vm;
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(ReviewEditModel model, VacancyUser user)
        {
            var vm = await GetReviewViewModelAsync(model.ReviewId, user);

            vm.SelectedFieldIdentifers = model.SelectedFieldIdentifers;

            vm.ReviewerComment = model.ReviewerComment;

            return vm;
        }

        public async Task<ReviewViewModel> GetReferralViewModelAsync(Guid reviewId, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            ValidateReviewStateForReferral(review);
            await EnsureUserIsAssignedAsync(review, user.UserId);

            var vm = await _mapper.MapFromVacancy(review.VacancySnapshot);
            vm.IsEditable = true;

            return vm;
        }

        private static void ValidateReviewStateForReferral(VacancyReview review)
        {
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {review.Id}");

            if (review.Status != ReviewStatus.UnderReview)
                throw new InvalidStateException($"Review is not in a correct state for referring. State: {review.Status}");

            if (review.VacancySnapshot == null)
                throw new NotFoundException($"Vacancy snapshot is null for review with id: {review.Id}");
        }

        private static void ValidateReviewStateForViewing(VacancyReview review)
        {
            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {review.Id}");

            if (review.Status != ReviewStatus.PendingReview && review.Status != ReviewStatus.UnderReview)
                throw new InvalidStateException($"Review is not in a correct state for viewing. State: {review.Status}");

            if (review.VacancySnapshot == null)
                throw new NotFoundException($"Vacancy snapshot is null for review with id: {review.Id}");
        }

        private async Task EnsureUserIsAssignedAsync(VacancyReview review, string userId)
        {
            var userReviews = await _vacancyClient.GetAssignedVacancyReviewsForUserAsync(userId);

            if(userReviews.Any(r => r.Id == review.Id) == false)
                throw new UnassignedVacancyReviewException($"You have been unassigned from {review.VacancyReference}");
        }

        public async Task<Guid?> AssignNextVacancyReviewAsync(VacancyUser user)
        {
            await _vacancyClient.AssignNextVacancyReviewAsync(user);

            var userVacancyReviews = await _vacancyClient.GetAssignedVacancyReviewsForUserAsync(user.UserId);

            return userVacancyReviews.FirstOrDefault()?.Id;
        }
    }
}