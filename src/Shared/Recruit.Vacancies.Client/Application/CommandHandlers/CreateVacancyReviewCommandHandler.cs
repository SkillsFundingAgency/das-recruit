﻿using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateVacancyReviewCommandHandler: IRequestHandler<CreateVacancyReviewCommand>
    {
        private readonly ILogger<CreateVacancyReviewCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _time;

        public CreateVacancyReviewCommandHandler(
            ILogger<CreateVacancyReviewCommandHandler> logger,
            IVacancyRepository vacancyRepository, 
            IVacancyReviewRepository vacancyReviewRepository, 
            IMessaging messaging, ITimeProvider time)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyReviewRepository = vacancyReviewRepository;
            _messaging = messaging;
            _time = time;
        }

        public async Task Handle(CreateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating vacancy review for vacancy {vacancyReference}.", message.VacancyReference);

            var vacancyTask = _vacancyRepository.GetVacancyAsync(message.VacancyReference);
            var previousReviewsTasks = _vacancyReviewRepository.GetForVacancyAsyc(message.VacancyReference);

            await Task.WhenAll(vacancyTask, previousReviewsTasks);

            var vacancy = vacancyTask.Result;
            var previousReviews = previousReviewsTasks.Result;
            
            var review = BuildNewReview(vacancy, previousReviews.Count);

            await _vacancyReviewRepository.CreateAsync(review);

            await _messaging.PublishEvent(new VacancyReviewCreatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                VacancyReference = message.VacancyReference,
                ReviewId =  review.Id
            });
        }

        private VacancyReview BuildNewReview(Vacancy vacancy, int previousReviewCount)
        {
            var review = new VacancyReview
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Title = vacancy.Title,
                Status = ReviewStatus.PendingReview,    // NOTE: This is temporary for private beta.
                CreatedDate = _time.Now,                // NOTE: This is temporary for private beta.
                EmployerAccountId = vacancy.EmployerAccountId,
                SubmittedByUser = vacancy.SubmittedByUser,
                SubmissionCount = previousReviewCount + 1
            };

            return review;
        }
    }
}
