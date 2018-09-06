using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class AssignNextVacanyReviewCommandHandler: IRequestHandler<AssignNextVacancyReviewCommand>
    {
        private readonly ILogger<AssignNextVacancyReviewCommand> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ITimeProvider _time;
        private readonly INextVacancyReviewService _nextVacancyReviewService;

        public AssignNextVacanyReviewCommandHandler(
            ILogger<AssignNextVacancyReviewCommand> logger,
            IVacancyReviewRepository vacancyReviewRepository, 
            ITimeProvider timeProvider,
            INextVacancyReviewService nextVacancyReviewService)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
            _time = timeProvider;
            _nextVacancyReviewService = nextVacancyReviewService;
        }

        public async Task Handle(AssignNextVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting assignment of next review for user {userId}.", message.User.UserId);

            var nextVacancyReview = await _nextVacancyReviewService.GetNextVacancyReviewAsync(message.User.UserId);

            if (nextVacancyReview == null)
            {
                _logger.LogInformation("No reviews to assign to user {userId}.", message.User.UserId);
                return;
            }
            
            nextVacancyReview.Status = ReviewStatus.UnderReview;
            nextVacancyReview.ReviewedByUser = message.User;
            nextVacancyReview.ReviewedDate = _time.Now;

            await _vacancyReviewRepository.UpdateAsync(nextVacancyReview);
        }
    }
}
