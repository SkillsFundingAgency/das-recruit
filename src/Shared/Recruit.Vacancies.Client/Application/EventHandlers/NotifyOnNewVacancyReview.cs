using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class NotifyOnNewVacancyReview : INotificationHandler<VacancyReviewCreatedEvent>
    {
        private readonly INotifyVacancyReviewUpdates _notifier;
        private readonly ILogger<NotifyOnNewVacancyReview> _logger;

        public NotifyOnNewVacancyReview(INotifyVacancyReviewUpdates notifier, ILogger<NotifyOnNewVacancyReview> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _notifier.VacancyReviewCreated(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewCreatedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}
