using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateQaDashboardOnReview : INotificationHandler<VacancyReviewCreatedEvent>,
                                             INotificationHandler<VacancyReviewApprovedEvent>,
                                             INotificationHandler<VacancyReviewReferredEvent>
    {
        private readonly ILogger<UpdateQaDashboardOnReview> _logger;
        private readonly IQaDashboardService _qaDashboardService;
        
        public UpdateQaDashboardOnReview(ILogger<UpdateQaDashboardOnReview> logger, IQaDashboardService qaDashboardService)
        {
            _logger = logger;
            _qaDashboardService = qaDashboardService;
        }

        public Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private Task Handle(IVacancyReviewEvent notification)
        {
            _logger.LogInformation("Handling {notificationType}", notification.GetType().Name);

            return _qaDashboardService.RebuildQaDashboardAsync();
        }
    }
}
