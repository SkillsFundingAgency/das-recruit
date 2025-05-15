using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Exceptions;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using UnassignedVacancyReviewException = Esfa.Recruit.Qa.Web.Exceptions.UnassignedVacancyReviewException;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class ReviewOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;
        private readonly ReviewMapper _mapper;
        private readonly IMessaging _messaging;

        public ReviewOrchestrator(IQaVacancyClient vacancyClient, ReviewMapper mapper, IMessaging messaging)
        {
            _vacancyClient = vacancyClient;
            _mapper = mapper;
            _messaging = messaging;
        }

        public async Task<Guid?> SubmitReviewAsync(ReviewEditModel m, VacancyUser user)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(m.ReviewId);
            
            await EnsureUserIsAssignedAsync(review, user.UserId);

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
                
            var manualQaFieldIndicators = _mapper.GetManualQaFieldIndicators(m);
            var selectedAutomatedQaRuleOutcomeIds = m.SelectedAutomatedQaResults.Select(Guid.Parse).ToList();

            if (m.IsRefer)
            {
                await _vacancyClient.ReferVacancyReviewAsync(m.ReviewId, m.ReviewerComment, manualQaFieldIndicators, selectedAutomatedQaRuleOutcomeIds);
            }
            else
            {
                var manualQaFieldEditIndicator = PopulateManualQaFieldEditIndicators(review, m, vacancy);

                if (manualQaFieldEditIndicator.Any())
                {
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                }
                
                await _messaging.SendCommandAsync(
                    new ApproveVacancyReviewCommand(m.ReviewId, m.ReviewerComment, manualQaFieldIndicators, selectedAutomatedQaRuleOutcomeIds, manualQaFieldEditIndicator));
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

            if(!string.IsNullOrEmpty(vm?.EmployerWebsiteUrl) && !vm.EmployerWebsiteUrl.StartsWith("http", true, null))
            {
                vm.EmployerWebsiteUrl = "https://" + $"{vm.EmployerWebsiteUrl}";
            }

            return vm;
        }

        public async Task<ReviewViewModel> GetReadonlyReviewViewModelAsync(Guid reviewId)
        {
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            if (review == null)
                throw new NotFoundException($"Unable to find review with id: {reviewId}");

            var vm = await _mapper.Map(review);
            vm.ReadOnly = true;

            if (!string.IsNullOrEmpty(vm?.EmployerWebsiteUrl) && !vm.EmployerWebsiteUrl.StartsWith("http", true, null))
            {
                vm.EmployerWebsiteUrl = "https://" + $"{vm.EmployerWebsiteUrl}";
            }

            if (!string.IsNullOrEmpty(vm?.ApplicationUrl) && !vm.ApplicationUrl.StartsWith("http", true, null))
            {
                vm.ApplicationUrl = "https://" + $"{vm.ApplicationUrl}";
            }

            return vm;
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(ReviewEditModel model, VacancyUser user)
        {
            var vm = await GetReviewViewModelAsync(model.ReviewId, user);

            foreach (var field in vm.FieldIdentifiers)
            {
                field.Checked = model.SelectedFieldIdentifiers.Contains(field.FieldIdentifier);
            }

            foreach (var automatedQaResult in vm.AutomatedQaResults)
            {
                automatedQaResult.Checked = model.SelectedAutomatedQaResults.Contains(automatedQaResult.OutcomeId);
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

            if (review == null)
            {
                return null;
            }

            return new UnassignReviewViewModel()
            {
                AdvisorName = review.ReviewedByUser?.Name,
                Title = review.Title
            };
        }

        public Task UnassignVacancyReviewAsync(Guid reviewId)
        {
            return _vacancyClient.UnassignVacancyReviewAsync(reviewId);
        }

        private List<ManualQaFieldEditIndicator> PopulateManualQaFieldEditIndicators(VacancyReview review,
            ReviewEditModel m, Vacancy vacancy)
        {
            var manualQaFieldEditIndicator = new List<ManualQaFieldEditIndicator>();
            if (review.VacancySnapshot.OutcomeDescription != m.OutcomeDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.OutcomeDescription),
                    BeforeEdit = review.VacancySnapshot.OutcomeDescription,
                    AfterEdit = m.OutcomeDescription
                });
                vacancy.OutcomeDescription = m.OutcomeDescription;
            }
            if (review.VacancySnapshot.TrainingDescription != m.TrainingDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.TrainingDescription),
                    BeforeEdit = review.VacancySnapshot.TrainingDescription,
                    AfterEdit = m.TrainingDescription
                });
                vacancy.TrainingDescription = m.TrainingDescription;
            }
            if (review.VacancySnapshot.AdditionalTrainingDescription != m.AdditionalTrainingDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.AdditionalTrainingDescription),
                    BeforeEdit = review.VacancySnapshot.AdditionalTrainingDescription,
                    AfterEdit = m.AdditionalTrainingDescription
                });
                vacancy.AdditionalTrainingDescription = m.AdditionalTrainingDescription;
            }
            if (review.VacancySnapshot.ShortDescription != m.ShortDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.ShortDescription),
                    BeforeEdit = review.VacancySnapshot.ShortDescription,
                    AfterEdit = m.ShortDescription
                });
                vacancy.ShortDescription = m.ShortDescription;
            }
            if (review.VacancySnapshot.Description != m.VacancyDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.VacancyDescription),
                    BeforeEdit = review.VacancySnapshot.Description,
                    AfterEdit = m.VacancyDescription
                });
                vacancy.Description = m.VacancyDescription;
            }
            if (review.VacancySnapshot.Wage.WorkingWeekDescription != m.WorkingWeekDescription)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.WorkingWeekDescription),
                    BeforeEdit = review.VacancySnapshot.Wage.WorkingWeekDescription,
                    AfterEdit = m.WorkingWeekDescription
                });
                vacancy.Wage.WorkingWeekDescription = m.WorkingWeekDescription;
            }
            if (review.VacancySnapshot.Wage.CompanyBenefitsInformation != m.CompanyBenefitsInformation)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.CompanyBenefitsInformation),
                    BeforeEdit = review.VacancySnapshot.Wage.CompanyBenefitsInformation,
                    AfterEdit = m.CompanyBenefitsInformation
                });
                vacancy.Wage.CompanyBenefitsInformation = m.CompanyBenefitsInformation;
            }
            if (review.VacancySnapshot.EmployerLocationInformation != m.EmployerLocationInformation)
            {
                manualQaFieldEditIndicator.Add(new ManualQaFieldEditIndicator
                {
                    FieldIdentifier = nameof(m.EmployerLocationInformation),
                    BeforeEdit = review.VacancySnapshot.EmployerLocationInformation,
                    AfterEdit = m.EmployerLocationInformation
                });
                vacancy.EmployerLocationInformation = m.EmployerLocationInformation;
            }

            return manualQaFieldEditIndicator;
        }
    }
}