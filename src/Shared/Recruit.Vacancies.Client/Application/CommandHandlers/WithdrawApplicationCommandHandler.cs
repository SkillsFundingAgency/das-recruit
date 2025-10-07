using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class WithdrawApplicationCommandHandler(
        ILogger<WithdrawApplicationCommandHandler> logger,
        ISqlDbRepository sqlDbRepository,
        IApplicationReviewRepositoryRunner applicationReviewRepositoryRunner,
        ITimeProvider timeProvider,
        IMessaging messaging)
        : IRequestHandler<WithdrawApplicationCommand, Unit>
    {
        public async Task<Unit> Handle(WithdrawApplicationCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Withdrawing application for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);

            var applicationReview = await sqlDbRepository.GetAsync(message.VacancyReference, message.CandidateId);

            if (applicationReview == null)
            {
                logger.LogInformation("Cannot find application to withdraw for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);
                return Unit.Value;
            }

            if (applicationReview.CanWithdraw == false)
            {
                logger.LogWarning("Cannot withdraw ApplicationReviewId:{applicationReviewId} as not in correct state", applicationReview.Id);
                return Unit.Value;
            }

            applicationReview.IsWithdrawn = true;
            applicationReview.WithdrawnDate = timeProvider.Now;
            applicationReview.Application = null;
            applicationReview.CandidateFeedback = null;

            await applicationReviewRepositoryRunner.UpdateAsync(applicationReview);

            await messaging.PublishEvent(new ApplicationReviewWithdrawnEvent
            {
                VacancyReference = applicationReview.VacancyReference
            });

            logger.LogInformation("Finished withdrawing application for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);
            
            return Unit.Value;
        }
    }
}
