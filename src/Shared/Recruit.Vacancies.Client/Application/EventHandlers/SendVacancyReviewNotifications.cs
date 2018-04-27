using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class SendVacancyReviewNotifications : INotificationHandler<VacancyReviewCreatedEvent>
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
                await _notifier.NewVacancyReview(notification.VacancyReference);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewCreatedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}
