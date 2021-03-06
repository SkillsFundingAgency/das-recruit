﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApplicationReviewCommandHandler : 
        IRequestHandler<ApplicationReviewSuccessfulCommand>,
        IRequestHandler<ApplicationReviewUnsuccessfulCommand>
    {
        private readonly ILogger<ApplicationReviewCommandHandler> _logger;        
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        private readonly AbstractValidator<ApplicationReview> _applicationReviewValidator;

        public ApplicationReviewCommandHandler(
            ILogger<ApplicationReviewCommandHandler> logger,            
            IApplicationReviewRepository applicationReviewRepository,
            ITimeProvider timeProvider,
            IMessaging messaging,
            AbstractValidator<ApplicationReview> applicationReviewValidator)
        {
            _logger = logger;            
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
            _applicationReviewValidator = applicationReviewValidator;
        }

        public Task Handle(ApplicationReviewSuccessfulCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting application review:{applicationReviewId} to successful", message.ApplicationReviewId);
            return Handle(message.ApplicationReviewId, message.User, ApplicationReviewStatus.Successful);
        }

        public Task Handle(ApplicationReviewUnsuccessfulCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting application review:{applicationReviewId} to unsuccessful", message.ApplicationReviewId);
            return Handle(message.ApplicationReviewId, message.User, ApplicationReviewStatus.Unsuccessful, message.CandidateFeedback);
        }

        private async Task Handle(Guid applicationReviewId, VacancyUser user, ApplicationReviewStatus status, string candidateFeedback = null)
        {
            var applicationReview = await _applicationReviewRepository.GetAsync(applicationReviewId);

            if(applicationReview.CanReview == false)
            {
                _logger.LogWarning("Cannot review ApplicationReviewId:{applicationReviewId} as not in correct state", applicationReview.Id);
                return;
            }

            applicationReview.Status = status;
            applicationReview.CandidateFeedback = candidateFeedback;
            applicationReview.StatusUpdatedDate = _timeProvider.Now;
            applicationReview.StatusUpdatedBy = user;

            Validate(applicationReview);
            
            await _applicationReviewRepository.UpdateAsync(applicationReview);

            await _messaging.PublishEvent(new ApplicationReviewedEvent
            {
                Status = applicationReview.Status,
                VacancyReference = applicationReview.VacancyReference,
                CandidateFeedback = applicationReview.CandidateFeedback,
                CandidateId = applicationReview.CandidateId
            });
        }

        private void Validate(ApplicationReview applicationReview)
        {
            var validationResult = _applicationReviewValidator.Validate(applicationReview);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
