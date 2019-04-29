using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class NotifyOnVacancyActionsEventHandler : INotificationHandler<VacancyClosedEvent>,
                                                        INotificationHandler<LiveVacancyUpdatedEvent>
    {
        private readonly INotifyVacancyUpdates _vacancyStatusNotifier;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<NotifyOnVacancyActionsEventHandler> _logger;

        public NotifyOnVacancyActionsEventHandler(ILogger<NotifyOnVacancyActionsEventHandler> logger, INotifyVacancyUpdates vacancyStatusNotifier, IVacancyRepository vacancyRepository, IVacancyReviewRepository vacancyReviewRepository)
        {
            _vacancyStatusNotifier = vacancyStatusNotifier;
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);

                if (vacancy.Status == VacancyStatus.Closed && vacancy.ClosedByUser != null)
                {
                    await _vacancyStatusNotifier.VacancyManuallyClosed(vacancy);
                }
            }
            catch (NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyClosedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }

        public async Task Handle(LiveVacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);

                switch (notification.UpdateKind)
                {
                    case LiveUpdateKind.ClosingDate:
                    case LiveUpdateKind.StartDate:
                    case LiveUpdateKind.ClosingDate | LiveUpdateKind.StartDate:
                        await _vacancyStatusNotifier.LiveVacancyChanged(vacancy);
                        break;
                    default:
                        break;
                }
            }
            catch (NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(LiveVacancyUpdatedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}