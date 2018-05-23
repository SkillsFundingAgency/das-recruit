using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class NotifyOnVacancyActions : INotificationHandler<VacancyApprovedEvent>,
                                          INotificationHandler<VacancyReferredEvent>
    {
        private readonly INotifyVacancyReviewUpdates _notifier;
        private readonly ILogger<NotifyOnVacancyActions> _logger;

        public NotifyOnVacancyActions(INotifyVacancyReviewUpdates notifier, ILogger<NotifyOnVacancyActions> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Handle(VacancyApprovedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending notification for vacancy {vacancyReference} approval.", notification.VacancyReference);
                await _notifier.VacancyReviewApproved(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewApprovedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }

        public async Task Handle(VacancyReferredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending notification for vacancy {vacancyReference} referral.", notification.VacancyReference);
                await _notifier.VacancyReviewReferred(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewReferredEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}