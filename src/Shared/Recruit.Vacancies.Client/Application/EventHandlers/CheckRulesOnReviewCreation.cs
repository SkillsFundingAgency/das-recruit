using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class CheckRulesOnReviewCreation : INotificationHandler<VacancyReviewCreatedEvent>
    {
        private readonly IVacancyService _vacancyService;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly ILogger<CheckRulesOnReviewCreation> _logger;

        public CheckRulesOnReviewCreation(
            IVacancyService vacancyService, IVacancyReviewRepository vacancyReviewRepository,
            ILogger<CheckRulesOnReviewCreation> logger)
        {
            _vacancyService = vacancyService;
            _vacancyReviewRepository = vacancyReviewRepository;
            _logger = logger;
        }

        public async Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Executing Automated QA on review {notification.ReviewId} for vacancy with reference {notification.VacancyReference}");
            var review = await _vacancyReviewRepository.GetAsync(notification.ReviewId);
            await _vacancyService.PerformRulesCheckAsync(review);
        }
    }
}
