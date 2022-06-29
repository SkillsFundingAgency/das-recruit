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
    public class UpdateEmployerDashboardOnChange : INotificationHandler<VacancyCreatedEvent>,
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
                                            INotificationHandler<SetupEmployerEvent>,
                                            INotificationHandler<VacancyReferredEvent>,
                                            INotificationHandler<VacancyTransferredEvent>,
                                            INotificationHandler<VacancyReviewWithdrawnEvent>
    {
        private readonly IEmployerDashboardProjectionService _dashboardService;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<UpdateEmployerDashboardOnChange> _logger;

        public UpdateEmployerDashboardOnChange(IEmployerDashboardProjectionService dashboardService, IVacancyRepository vacancyRepository, ILogger<UpdateEmployerDashboardOnChange> logger)
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

        public Task Handle(SetupEmployerEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReferredEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public async Task Handle(VacancyTransferredEvent notification, CancellationToken cancellationToken)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyReference);

            _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId} and vacancyReference: {vacancyReference}", notification.GetType().Name, vacancy.EmployerAccountId, notification.VacancyReference);
            await _dashboardService.ReBuildDashboardAsync(vacancy.EmployerAccountId);
        }

        public Task Handle(VacancyReviewWithdrawnEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private Task Handle(IEmployerEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId}", notification.GetType().Name, notification.EmployerAccountId);
            return _dashboardService.ReBuildDashboardAsync(notification.EmployerAccountId);
        }

        private async Task Handle(IApplicationReviewEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyReference);

            if (vacancy.VacancyType.GetValueOrDefault() == VacancyType.Traineeship)
            {
                return;
            }
            _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId} and vacancyReference: {vacancyReference}", notification.GetType().Name, vacancy.EmployerAccountId, notification.VacancyReference);
            await _dashboardService.ReBuildDashboardAsync(vacancy.EmployerAccountId);
        }

        private async Task Handle(IVacancyEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);

           _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId} and vacancyId: {vacancyId}", notification.GetType().Name, vacancy.EmployerAccountId, notification.VacancyId);
           await _dashboardService.ReBuildDashboardAsync(vacancy.EmployerAccountId);
        }
    }
}
