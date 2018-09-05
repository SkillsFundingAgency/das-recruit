using System;
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
        private readonly ITimeProvider _timeProvider;

        public ReviewOrchestrator(IQaVacancyClient vacancyClient, ReviewMapper mapper, ITimeProvider timeProvider)
        {
            _vacancyClient = vacancyClient;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        public async Task<Guid?> ApproveReviewAsync(ReviewEditModel m, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(m.ReviewId);
            EnsureUserIsAssigned(review, user.UserId);

            await _vacancyClient.ApproveVacancyReviewAsync(m.ReviewId, m.ReviewerComment);

            var nextVacancyReview = await _vacancyClient.AssignNextVacancyReviewAsync(user);

            return nextVacancyReview?.Id;
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
            EnsureUserIsAssigned(review, user.UserId);

            var vm = await _mapper.MapFromVacancy(review.VacancySnapshot);

            return vm;
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(ReviewEditModel model, VacancyUser user)
        {
            var vm = await GetReviewViewModelAsync(model.ReviewId, user);

            vm.EmployerNameChecked = model.EmployerNameChecked;
            vm.ShortDescriptionChecked = model.ShortDescriptionChecked;
            vm.ClosingDateChecked = model.ClosingDateChecked;
            vm.WorkingWeekChecked = model.WorkingWeekChecked;
            vm.WageChecked = model.WageChecked;
            vm.ExpectedDurationChecked = model.ExpectedDurationChecked;
            vm.PossibleStartDateChecked = model.PossibleStartDateChecked;
            vm.TrainingLevelChecked = model.TrainingLevelChecked;
            vm.NumberOfPositionsChecked = model.NumberOfPositionsChecked;
            vm.VacancyDescriptionChecked = model.VacancyDescriptionChecked;
            vm.TrainingDescriptionChecked = model.TrainingDescriptionChecked;
            vm.OutcomeDescriptionChecked = model.OutcomeDescriptionChecked;
            vm.SkillsChecked = model.SkillsChecked;
            vm.QualificationsChecked = model.QualificationsChecked;
            vm.ThingsToConsiderChecked = model.ThingsToConsiderChecked;
            vm.EmployerDescriptionChecked = model.EmployerDescriptionChecked;
            vm.EmployerWebsiteUrlChecked = model.EmployerWebsiteUrlChecked;
            vm.ContactChecked = model.ContactChecked;
            vm.EmployerAddressChecked = model.EmployerAddressChecked;
            vm.ProviderChecked = model.ProviderChecked;
            vm.TrainingChecked = model.TrainingChecked;
            vm.ApplicationProcessChecked = model.ApplicationProcessChecked;
            vm.ReviewerComment = model.ReviewerComment;

            return vm;
        }

        public async Task<ReviewViewModel> GetReferralViewModelAsync(Guid reviewId, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            ValidateReviewStateForReferral(review);
            EnsureUserIsAssigned(review, user.UserId);

            if (review.ManualOutcome != ManualQaOutcome.Referred)
            {
                await _vacancyClient.ReferVacancyReviewAsync(review.Id);
            }

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

        private void EnsureUserIsAssigned(VacancyReview review, string userId)
        {
            if (review.ReviewedByUser?.UserId != userId || review.AssignationExpiry < _timeProvider.Now)
                throw new UnassignedVacancyReviewException($"You have been unassigned from {review.VacancyReference}");
        }
    }
}