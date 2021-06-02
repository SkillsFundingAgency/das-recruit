using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateProviderDashboardOnChangeEventHandler : INotificationHandler<VacancyCreatedEvent>,
                                            INotificationHandler<DraftVacancyUpdatedEvent>,
                                            INotificationHandler<VacancySubmittedEvent>,
                                            INotificationHandler<VacancyReviewedEvent>,
                                            INotificationHandler<VacancyDeletedEvent>,
                                            INotificationHandler<VacancyPublishedEvent>,
                                            INotificationHandler<VacancyClosedEvent>,
                                            INotificationHandler<ApplicationReviewCreatedEvent>,
                                            INotificationHandler<ApplicationReviewWithdrawnEvent>,
                                            INotificationHandler<ApplicationReviewDeletedEvent>,
                                            INotificationHandler<ApplicationReviewedEvent>,
                                            INotificationHandler<SetupProviderEvent>,
                                            INotificationHandler<VacancyReferredEvent>,
                                            INotificationHandler<VacancyRejectedEvent>,
                                            INotificationHandler<VacancyTransferredEvent>,
                                            INotificationHandler<VacancyReviewWithdrawnEvent>
    {
        private readonly IProviderDashboardProjectionService _dashboardService;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<UpdateProviderDashboardOnChangeEventHandler> _logger;

        public UpdateProviderDashboardOnChangeEventHandler(IProviderDashboardProjectionService dashboardService, IVacancyRepository vacancyRepository, ILogger<UpdateProviderDashboardOnChangeEventHandler> logger)
        {
            _dashboardService = dashboardService;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(DraftVacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReviewedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyPublishedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewWithdrawnEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewDeletedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(SetupProviderEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReferredEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyRejectedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public async Task Handle(VacancyTransferredEvent notification, CancellationToken cancellationToken)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyReference);

            _logger.LogInformation("Handling {eventType} for ukprn: {ukprn} and vacancyReference: {vacancyReference}", notification.GetType().Name, vacancy.TrainingProvider.Ukprn.Value, notification.VacancyReference);
            await _dashboardService.ReBuildDashboardAsync(vacancy.TrainingProvider.Ukprn.GetValueOrDefault());
        }

        public Task Handle(VacancyReviewWithdrawnEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private Task Handle(IProviderEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            _logger.LogInformation("Handling {eventType} for ukprn: {ukprn}", notification.GetType().Name, notification.Ukprn);
            return _dashboardService.ReBuildDashboardAsync(notification.Ukprn);
        }

        private async Task Handle(IApplicationReviewEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyReference);

            _logger.LogInformation("Handling {eventType} for ukprn: {ukprn} and vacancyReference: {vacancyReference}", notification.GetType().Name, vacancy.TrainingProvider.Ukprn.Value, notification.VacancyReference);
            await _dashboardService.ReBuildDashboardAsync(vacancy.TrainingProvider.Ukprn.Value);
        }

        private async Task Handle(IVacancyEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);

            if (vacancy.TrainingProvider != null)
            {
                _logger.LogInformation("Handling {eventType} for ukprn: {ukprn} and vacancyId: {vacancyId}", notification.GetType().Name, vacancy.TrainingProvider.Ukprn.Value, notification.VacancyId);
                await _dashboardService.ReBuildDashboardAsync(vacancy.TrainingProvider.Ukprn.Value);
            }
        }
    }
}
