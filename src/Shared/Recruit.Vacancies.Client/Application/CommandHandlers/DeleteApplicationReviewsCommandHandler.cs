using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class DeleteApplicationReviewsCommandHandler : IRequestHandler<DeleteApplicationReviewsCommand, Unit>
    {
        private readonly ILogger<DeleteApplicationReviewsCommandHandler> _logger;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IApplicationReviewQuery _applicationReviewQuery;
        private readonly IMessaging _messaging;

        public DeleteApplicationReviewsCommandHandler(
            ILogger<DeleteApplicationReviewsCommandHandler> logger,
            IApplicationReviewRepository applicationReviewRepository,
            IApplicationReviewQuery applicationReviewQuery,
            IMessaging messaging)
        {
            _logger = logger;
            _applicationReviewRepository = applicationReviewRepository;
            _applicationReviewQuery = applicationReviewQuery;
            _messaging = messaging;
        }

        public async Task<Unit> Handle(DeleteApplicationReviewsCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting all application reviews for candidateId:{candidateId}",message.CandidateId);

            var candidateApplicationReviews = await _applicationReviewQuery.GetForCandidateAsync(message.CandidateId);

            foreach (var applicationReview in candidateApplicationReviews)
            {
                await _applicationReviewRepository.HardDelete(applicationReview.Id);

                await _messaging.PublishEvent(new ApplicationReviewDeletedEvent
                {
                    VacancyReference = applicationReview.VacancyReference
                });

                _logger.LogInformation("Deleted application review id:{applicationReviewId} for candidateId:{candidateId}", applicationReview.Id, message.CandidateId);
            }

            _logger.LogInformation($"Deleted {candidateApplicationReviews.Count} application reviews for candidateId:{{candidateId}}", message.CandidateId);
            
            return Unit.Value;
        }
    }
}
