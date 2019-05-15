using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class NotifyOnVacancyReviewActionsEventHandler : INotificationHandler<VacancyReviewApprovedEvent>,
                                        INotificationHandler<VacancyReviewReferredEvent>,
                                        INotificationHandler<VacancyReviewCreatedEvent>
    {
        private readonly INotifyVacancyReviewUpdates _reviewStatusNotifier;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<NotifyOnVacancyReviewActionsEventHandler> _logger;

        public NotifyOnVacancyReviewActionsEventHandler(INotifyVacancyReviewUpdates reviewStatusNotifier, IVacancyRepository vacancyRepository, IVacancyReviewRepository vacancyReviewRepository, ILogger<NotifyOnVacancyReviewActionsEventHandler> logger)
        {
            _reviewStatusNotifier = reviewStatusNotifier;
            _vacancyReviewRepository = vacancyReviewRepository;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public async Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending notification for vacancy {vacancyReference} approval.", notification.VacancyReference);

                var vacancyReview = await _vacancyReviewRepository.GetAsync(notification.ReviewId);

                await _reviewStatusNotifier.VacancyReviewApproved(vacancyReview);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewApprovedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }

        public async Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending notification for vacancy {vacancyReference} referral.", notification.VacancyReference);

                var vacancyReview = await _vacancyReviewRepository.GetAsync(notification.ReviewId);

                await _reviewStatusNotifier.VacancyReviewReferred(vacancyReview);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewReferredEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }

        public async Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var vacancyReview = await _vacancyReviewRepository.GetAsync(notification.ReviewId);

                await _reviewStatusNotifier.VacancyReviewCreated(vacancyReview);
            }
            catch(NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notification for {nameof(VacancyReviewCreatedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
            }
        }
    }
}