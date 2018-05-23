using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateVacancyReviewCommandHandler: IRequestHandler<UpdateVacancyReviewCommand>
    {
        private readonly ILogger<UpdateVacancyReviewCommandHandler> _logger;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;

        public UpdateVacancyReviewCommandHandler(ILogger<UpdateVacancyReviewCommandHandler> logger, IVacancyReviewRepository vacancyReviewRepository)
        {
            _logger = logger;
            _vacancyReviewRepository = vacancyReviewRepository;
        }

        public Task Handle(UpdateVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating review {reviewId}", message.Review.Id);
            
            return _vacancyReviewRepository.UpdateAsync(message.Review);
        }
    }
}
