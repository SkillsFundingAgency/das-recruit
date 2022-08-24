using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UnassignVacancyReviewCommandHandler : IRequestHandler<UnassignVacancyReviewCommand, Unit>
    {
        private readonly IVacancyReviewRepository _repository;
        private readonly ILogger<UnassignVacancyReviewCommandHandler> _logger;

        public UnassignVacancyReviewCommandHandler(
            IVacancyReviewRepository repository, ILogger<UnassignVacancyReviewCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Unit> Handle(UnassignVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to unassign review {reviewId}.", message.ReviewId);

            var review = await _repository.GetAsync(message.ReviewId);

            if (!review.CanUnassign)
            {
                _logger.LogWarning($"Unable to unassign {review.ReviewedByUser.Name} from review {message.ReviewId}, it may already be unassigned.");
                return Unit.Value;
            }

            review.Status = ReviewStatus.PendingReview;
            review.ReviewedDate = null;
            review.ReviewedByUser = null;

            await _repository.UpdateAsync(review);
            
            return Unit.Value;
        }
    }
}
