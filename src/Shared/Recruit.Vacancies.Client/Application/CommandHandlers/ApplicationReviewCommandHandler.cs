using System;
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
        IRequestHandler<ApplicationReviewStatusEditCommand, bool>
    {
        private readonly ILogger<ApplicationReviewCommandHandler> _logger;        
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        private readonly AbstractValidator<ApplicationReview> _applicationReviewValidator;

        public ApplicationReviewCommandHandler(
            ILogger<ApplicationReviewCommandHandler> logger,            
            IApplicationReviewRepository applicationReviewRepository,
            IVacancyRepository vacancyRepository,
            ITimeProvider timeProvider,
            IMessaging messaging,
            AbstractValidator<ApplicationReview> applicationReviewValidator)
        {
            _logger = logger;            
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
            _applicationReviewValidator = applicationReviewValidator;
            _vacancyRepository = vacancyRepository;
        }

        public async Task<bool> Handle(ApplicationReviewStatusEditCommand message, CancellationToken cancellationToke)
        {
            var applicationReview = await _applicationReviewRepository.GetAsync(message.ApplicationReviewId);

            if(applicationReview.CanReview == false)
            {
                _logger.LogWarning("Cannot review ApplicationReviewId:{applicationReviewId} as not in correct state", applicationReview.Id);
                return false;
            }

            applicationReview.Status = message.Outcome.Value;
            applicationReview.CandidateFeedback = message.CandidateFeedback;
            applicationReview.StatusUpdatedDate = _timeProvider.Now;
            applicationReview.StatusUpdatedBy = message.User;

            if (applicationReview.Status == ApplicationReviewStatus.EmployerInterviewing || applicationReview.Status == ApplicationReviewStatus.EmployerUnsuccessful)
            {
                applicationReview.ReviewedDate = _timeProvider.Now;
            }

            if (applicationReview.Status == ApplicationReviewStatus.EmployerInterviewing) 
            {
                applicationReview.HasEverBeenEmployerInterviewing = true;
            }

            Validate(applicationReview);
            _logger.LogInformation("Setting application review:{applicationReviewId} to {status}", message.ApplicationReviewId, message.Outcome.Value);

            await _applicationReviewRepository.UpdateAsync(applicationReview);
            
            await _messaging.PublishEvent(new ApplicationReviewedEvent
            {
                Status = applicationReview.Status,
                VacancyReference = applicationReview.VacancyReference,
                CandidateFeedback = applicationReview.CandidateFeedback,
                CandidateId = applicationReview.CandidateId
            });

            var shouldMakeOthersUnsuccessful = await CheckForPositionsFilledAsync(message.Outcome, applicationReview.VacancyReference);

            return shouldMakeOthersUnsuccessful;
        }

        private void Validate(ApplicationReview applicationReview)
        {
            var validationResult = _applicationReviewValidator.Validate(applicationReview);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private async Task<bool> CheckForPositionsFilledAsync(ApplicationReviewStatus? status, long vacancyReference)
        {
            var shouldMakeOthersUnsuccessful = false;
            if (status == ApplicationReviewStatus.Successful)
            {
                var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

                var successfulApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.Successful);

                if (vacancy.NumberOfPositions <= successfulApplications.Count)
                {
                    var newApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.New);
                    var interviewingApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.Interviewing);
                    var inReviewApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.InReview);
                    var employerInterviewingApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.EmployerInterviewing);
                    var employerUnsuccessflApplications = await _applicationReviewRepository.GetByStatusAsync(vacancyReference, ApplicationReviewStatus.EmployerUnsuccessful);
                    var applicationsToMakeUnsuccessful = newApplications.Count + interviewingApplications.Count + inReviewApplications.Count + employerInterviewingApplications.Count + employerUnsuccessflApplications.Count;
                    shouldMakeOthersUnsuccessful = (applicationsToMakeUnsuccessful > 0) ? true : false;
                }
            }

            return shouldMakeOthersUnsuccessful;
        }
    }
}
