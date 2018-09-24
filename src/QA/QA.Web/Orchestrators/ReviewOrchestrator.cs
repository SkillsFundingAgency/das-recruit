using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Exceptions;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using UnassignedVacancyReviewException = Esfa.Recruit.Qa.Web.Exceptions.UnassignedVacancyReviewException;

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

        public async Task<Guid?> SubmitReviewAsync(ReviewEditModel m, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(m.ReviewId);
            await EnsureUserIsAssignedAsync(review, user.UserId);

            var manualQaFieldIndicators = _mapper.GetManualQaFieldIndicators(m);

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

        public async Task<ReviewViewModel> GetReviewViewModelAsync(Guid reviewId, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {reviewId}");

            ValidateReviewStateForViewing(review);

            if (_vacancyClient.VacancyReviewCanBeAssigned(review))
            {
                await _vacancyClient.AssignVacancyReviewAsync(user, review.Id);
                review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            }

            await EnsureUserIsAssignedAsync(review, user.UserId);

            var vm = await _mapper.Map(review);

            return vm;
        }

        public async Task<ReviewViewModel> GetReadonlyReviewViewModelAsync(Guid reviewId)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {reviewId}");

            var vm = await _mapper.Map(review);

            return vm;
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(ReviewEditModel model, VacancyUser user)
        {
            var vm = await GetReviewViewModelAsync(model.ReviewId, user);

            foreach (var field in vm.FieldIdentifiers)
            {
                field.Checked = model.SelectedFieldIdentifiers.Contains(field.FieldIdentifier);
            }

            vm.ReviewerComment = model.ReviewerComment;

            return vm;
        }

        private static void ValidateReviewStateForViewing(VacancyReview review)
        {
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

        public async Task<UnassignReviewViewModel> GetUnassignReviewViewModelAsync(Guid reviewId)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            return new UnassignReviewViewModel()
            {
                AdvisorName = review.ReviewedByUser.Name,
                Title = review.Title
            };
        }

        public Task UnassignVacancyReviewAsync(Guid reviewId)
        {
            return _vacancyClient.UnassignVacancyReviewAsync(reviewId);
        }
    }
}