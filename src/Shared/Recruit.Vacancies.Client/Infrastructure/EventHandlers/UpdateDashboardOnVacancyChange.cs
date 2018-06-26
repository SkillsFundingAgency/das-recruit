using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateDashboardOnVacancyChange : INotificationHandler<VacancyCreatedEvent>,
                                                    INotificationHandler<VacancyDraftUpdatedEvent>,
                                                    INotificationHandler<VacancySubmittedEvent>,
                                                    INotificationHandler<VacancyDeletedEvent>,
                                                    INotificationHandler<VacancyLiveEvent>,
                                                    INotificationHandler<VacancyClosedEvent>,
                                                    INotificationHandler<ApplicationReviewCreatedEvent>,
                                                    INotificationHandler<ApplicationReviewSuccessfulEvent>
    {
        
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<UpdateDashboardOnVacancyChange> _logger;
        

        public UpdateDashboardOnVacancyChange(IDashboardService dashboardService, ILogger<UpdateDashboardOnVacancyChange> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyDraftUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyLiveEvent notification, CancellationToken cancellationToken)
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

        public Task Handle(ApplicationReviewSuccessfulEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private Task Handle(IVacancyEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");
            
            _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId} and vacancyId: {vacancyId}", notification.GetType().Name, notification.EmployerAccountId, notification.VacancyId);
            return _dashboardService.ReBuildDashboardAsync(notification.EmployerAccountId);
        }
    }
}
