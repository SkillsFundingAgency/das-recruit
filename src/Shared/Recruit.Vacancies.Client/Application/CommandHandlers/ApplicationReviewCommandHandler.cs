using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApplicationReviewCommandHandler(
        ILogger<ApplicationReviewCommandHandler> logger,
        ISqlDbRepository sqlDbRepository,
        IVacancyRepository vacancyRepository,
        ITimeProvider timeProvider,
        IMessaging messaging,
        IOuterApiClient outerApiClient,
        IApplicationReviewRepositoryRunner applicationReviewRepositoryRunner,
        AbstractValidator<ApplicationReview> applicationReviewValidator)
        : IRequestHandler<ApplicationReviewStatusEditCommand, bool>
    {
        public async Task<bool> Handle(ApplicationReviewStatusEditCommand message, CancellationToken cancellationToke)
        {
            var applicationReview = await sqlDbRepository.GetAsync(message.ApplicationReviewId);

            if (applicationReview.CanReview == false)
            {
                logger.LogWarning("Cannot review ApplicationReviewId:{applicationReviewId} as not in correct state", applicationReview.Id);
                return false;
            }

            applicationReview.Status = message.Outcome.Value;
            applicationReview.CandidateFeedback = message.CandidateFeedback;
            applicationReview.StatusUpdatedDate = timeProvider.Now;
            applicationReview.StatusUpdatedBy = message.User;

            if (applicationReview.Status == ApplicationReviewStatus.EmployerInterviewing || applicationReview.Status == ApplicationReviewStatus.EmployerUnsuccessful)
            {
                applicationReview.ReviewedDate = timeProvider.Now;
            }

            if (applicationReview.Status == ApplicationReviewStatus.EmployerInterviewing) 
            {
                applicationReview.HasEverBeenEmployerInterviewing = true;
            }

            if (applicationReview.Status == ApplicationReviewStatus.EmployerUnsuccessful)
            {
                applicationReview.EmployerFeedback = message.CandidateFeedback;
            }

            Validate(applicationReview);
            logger.LogInformation("Setting application review:{applicationReviewId} to {status}", message.ApplicationReviewId, message.Outcome.Value);

            await applicationReviewRepositoryRunner.UpdateAsync(applicationReview);

            if (applicationReview.Status is not (ApplicationReviewStatus.Successful or ApplicationReviewStatus.Unsuccessful))
            {
                return false;
            }

            var vacancy = await vacancyRepository.GetVacancyAsync(applicationReview.VacancyReference);
            
            if (!applicationReview.Application.IsFaaV2Application)
            {
                await messaging.PublishEvent(new ApplicationReviewedEvent
                {
                    Status = applicationReview.Status,
                    VacancyReference = applicationReview.VacancyReference,
                    CandidateFeedback = applicationReview.CandidateFeedback,
                    CandidateId = applicationReview.CandidateId
                });    
            }
            
            await outerApiClient.Post(new PostApplicationStatusRequest(applicationReview.Application.CandidateId,
                applicationReview.Application.ApplicationId, new PostApplicationStatus
                {
                    VacancyReference = applicationReview.VacancyReference,
                    Status = applicationReview.Status.ToString(),
                    CandidateFeedback = applicationReview.CandidateFeedback,
                    VacancyTitle = vacancy.Title,
                    VacancyEmployerName = vacancy.EmployerName,
                    VacancyLocation = vacancy.GetVacancyLocation()
                }));
            
            return await CheckForPositionsFilledAsync(message.Outcome,vacancy, applicationReview.VacancyReference);
        }

        private void Validate(ApplicationReview applicationReview)
        {
            var validationResult = applicationReviewValidator.Validate(applicationReview);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private async Task<bool> CheckForPositionsFilledAsync(
            ApplicationReviewStatus? status,
            Vacancy vacancy,
            long vacancyReference)
        {
            // Only check if a new successful application was added
            if (status != ApplicationReviewStatus.Successful)
                return false;

            var counts = await sqlDbRepository.GetApplicationReviewsCountByVacancyReferenceAsync(vacancyReference);

            // If all positions are filled
            if (!(counts.SuccessfulApplications >= vacancy.NumberOfPositions)) return false;
            
            // Count any remaining applications that aren't successful
            int remainingApplications =
                counts.NewApplications +
                counts.InterviewingApplications +
                counts.InReviewApplications +
                counts.EmployerInterviewingApplications +
                counts.UnsuccessfulApplications;

            return remainingApplications > 0;
        }
    }
}
