using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateVacancyReviewCommandHandler: IRequestHandler<CreateVacancyReviewCommand, Unit>
    {
        private readonly ILogger<CreateVacancyReviewCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _time;
        private readonly ISlaService _slaService;
        private readonly IVacancyComparerService _vacancyComparerService;

        public CreateVacancyReviewCommandHandler(
            ILogger<CreateVacancyReviewCommandHandler> logger,
            IVacancyRepository vacancyRepository,
            IVacancyReviewRepository vacancyReviewRepository,
            IVacancyReviewQuery vacancyReviewQuery, 
            IMessaging messaging, 
            ITimeProvider time,
            ISlaService slaService,
            IVacancyComparerService vacancyComparerService)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyReviewQuery = vacancyReviewQuery;
            _messaging = messaging;
            _time = time;
            _slaService = slaService;
            _vacancyComparerService = vacancyComparerService;
        }

        public async Task<Unit> Handle(CreateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating vacancy review for vacancy {vacancyReference}.", message.VacancyReference);

            var vacancyTask = _vacancyRepository.GetVacancyAsync(message.VacancyReference);
            var previousReviewsTask = _vacancyReviewQuery.GetForVacancyAsync(message.VacancyReference);

            await Task.WhenAll(vacancyTask, previousReviewsTask);

            var vacancy = vacancyTask.Result;
            var previousReviews = previousReviewsTask.Result;

            //Defensive code, just in case the message is republished due to an error after creating the review. 
            var activePreviousReview = previousReviews.FirstOrDefault(r => r.Status != ReviewStatus.Closed);
            if (activePreviousReview != null)
            {
                _logger.LogWarning($"Cannot create review for vacancy {message.VacancyReference} as an active review {activePreviousReview.Id} already exists.");
                return Unit.Value;
            }

            var slaDeadline = await _slaService.GetSlaDeadlineAsync(vacancy.SubmittedDate.Value);

            var updatedFields = GetUpdatedFields(vacancy, previousReviews);

            var review = BuildNewReview(vacancy, previousReviews.Count, slaDeadline, updatedFields, previousReviews.OrderByDescending(c=>c.SubmissionCount).FirstOrDefault());

            await _vacancyReviewRepository.CreateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewCreatedEvent
            {
                VacancyReference = message.VacancyReference,
                ReviewId =  review.Id
            });
            return Unit.Value;
        }

        private VacancyReview BuildNewReview(Vacancy vacancy, int previousReviewCount, DateTime slaDeadline, List<string> updatedFieldIdentifiers, VacancyReview previousReview)
        {
            var review = new VacancyReview
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Title = vacancy.Title,
                Status = ReviewStatus.New,    
                CreatedDate = _time.Now,
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

            var comparison = _vacancyComparerService.Compare(vacancy, previousReview.VacancySnapshot);

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
