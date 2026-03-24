using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateVacancyReviewCommandHandler(
        ILogger<CreateVacancyReviewCommandHandler> logger,
        IVacancyRepository vacancyRepository,
        IVacancyReviewRepository vacancyReviewRepository,
        IVacancyReviewQuery vacancyReviewQuery,
        IVacancyService vacancyService,
        ITimeProvider time,
        ISlaService slaService,
        IVacancyComparerService vacancyComparerService)
        : IRequestHandler<CreateVacancyReviewCommand, Unit>
    {
        public async Task<Unit> Handle(CreateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating vacancy review for vacancy {vacancyReference}.", message.VacancyReference);

            var vacancyTask = vacancyRepository.GetVacancyAsync(message.VacancyReference);
            var previousReviewsTask = vacancyReviewQuery.GetForVacancyAsync(message.VacancyReference);

            await Task.WhenAll(vacancyTask, previousReviewsTask);

            var vacancy = vacancyTask.Result;
            var previousReviews = previousReviewsTask.Result;

            //Defensive code, just in case the message is republished due to an error after creating the review. 
            var activePreviousReview = previousReviews.FirstOrDefault(r => r.Status != ReviewStatus.Closed);
            if (activePreviousReview != null)
            {
                logger.LogWarning("Cannot create review for vacancy {MessageVacancyReference} as an active review {Guid} already exists.", message.VacancyReference, activePreviousReview.Id);
                return Unit.Value;
            }

            var slaDeadline = await slaService.GetSlaDeadlineAsync(vacancy.SubmittedDate.GetValueOrDefault());

            var updatedFields = GetUpdatedFields(vacancy, previousReviews);

            var review = BuildNewReview(vacancy, previousReviews.Count, slaDeadline, updatedFields, previousReviews.OrderByDescending(c=>c.SubmissionCount).FirstOrDefault());

            await vacancyReviewRepository.CreateAsync(review);

            await vacancyService.PerformRulesCheckAsync(review.Id);
            
            return Unit.Value;
        }

        private VacancyReview BuildNewReview(Vacancy vacancy, int previousReviewCount, DateTime slaDeadline, List<string> updatedFieldIdentifiers, VacancyReview previousReview)
        {
            var review = new VacancyReview
            {
                Id = Guid.NewGuid(),
                VacancyReference = vacancy.VacancyReference.GetValueOrDefault(),
                Title = vacancy.Title,
                Status = ReviewStatus.New,    
                CreatedDate = time.Now,
                SubmittedByUser = vacancy.SubmittedByUser,
                SubmissionCount = previousReviewCount + 1,
                SlaDeadline = slaDeadline,
                VacancySnapshot = vacancy,
                UpdatedFieldIdentifiers = updatedFieldIdentifiers,
                DismissedAutomatedQaOutcomeIndicators = previousReview?.DismissedAutomatedQaOutcomeIndicators.Except(updatedFieldIdentifiers).ToList()
            };

            return review;
        }

        private List<string> GetUpdatedFields(Vacancy vacancy, IEnumerable<VacancyReview> allReviewsForVacancy)
        {
            var previousReview = GetPreviousReferredVacancyReview(allReviewsForVacancy);
            if(previousReview == null)
                return new List<string>();

            var comparison = vacancyComparerService.Compare(vacancy, previousReview.VacancySnapshot);

            return comparison.Fields
                .Where(f => f.AreEqual == false)
                .Select(f => f.FieldName)
                .ToList();
        }

        private VacancyReview GetPreviousReferredVacancyReview(IEnumerable<VacancyReview> allReviewsForVacancy)
        {
            return allReviewsForVacancy.Where(r => r.Status == ReviewStatus.Closed &&
                                         r.ManualOutcome == ManualQaOutcome.Referred)
                .OrderByDescending(r => r.ClosedDate)
                .FirstOrDefault();
        }
    }
}
