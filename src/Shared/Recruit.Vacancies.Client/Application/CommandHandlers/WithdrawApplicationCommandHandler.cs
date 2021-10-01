using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class WithdrawApplicationCommandHandler : IRequestHandler<WithdrawApplicationCommand, Unit>
    {
        private readonly ILogger<WithdrawApplicationCommandHandler> _logger;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public WithdrawApplicationCommandHandler(
            ILogger<WithdrawApplicationCommandHandler> logger, 
            IApplicationReviewRepository applicationReviewRepository, 
            ITimeProvider timeProvider,
            IMessaging messaging)
        {
            _logger = logger;
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task<Unit> Handle(WithdrawApplicationCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Withdrawing application for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);

            var applicationReview = await _applicationReviewRepository.GetAsync(message.VacancyReference, message.CandidateId);

            if (applicationReview == null)
            {
                _logger.LogInformation("Cannot find application to withdraw for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);
                return Unit.Value;
            }

            if (applicationReview.CanWithdraw == false)
            {
                _logger.LogWarning("Cannot withdraw ApplicationReviewId:{applicationReviewId} as not in correct state", applicationReview.Id);
                return Unit.Value;
            }

            applicationReview.IsWithdrawn = true;
            applicationReview.WithdrawnDate = _timeProvider.Now;
            applicationReview.Application = null;
            applicationReview.CandidateFeedback = null;

            await _applicationReviewRepository.UpdateAsync(applicationReview);

            await _messaging.PublishEvent(new ApplicationReviewWithdrawnEvent
            {
                VacancyReference = applicationReview.VacancyReference
            });

            _logger.LogInformation("Finished withdrawing application for vacancyReference:{vacancyReference} and candidateId:{candidateId}", message.VacancyReference, message.CandidateId);
            
            return Unit.Value;
        }
    }
}
