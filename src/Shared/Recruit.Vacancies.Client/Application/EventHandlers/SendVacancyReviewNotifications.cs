using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class SendVacancyReviewNotifications : INotificationHandler<VacancyReviewCreatedEvent>,
                                                  INotificationHandler<VacancyReviewReferredEvent>,
                                                  INotificationHandler<VacancyReviewApprovedEvent>
    {
        private readonly INotifyVacancyReviewUpdates _notifier;
        private readonly ILogger<SendVacancyReviewNotifications> _logger;

        public SendVacancyReviewNotifications(INotifyVacancyReviewUpdates notifier, ILogger<SendVacancyReviewNotifications> logger)
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

        public async Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _notifier.VacancyReviewReferred(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewReferredEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }

        public async Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _notifier.VacancyReviewApproved(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewApprovedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}
