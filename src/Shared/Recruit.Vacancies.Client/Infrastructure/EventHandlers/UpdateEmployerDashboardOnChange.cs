using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateEmployerDashboardOnChange :  INotificationHandler<VacancyCreatedEvent>,
                                            INotificationHandler<DraftVacancyUpdatedEvent>,
                                            INotificationHandler<VacancySubmittedEvent>,
                                            INotificationHandler<VacancyDeletedEvent>,
                                            INotificationHandler<VacancyPublishedEvent>,
                                            INotificationHandler<VacancyClosedEvent>,
                                            INotificationHandler<ApplicationReviewCreatedEvent>,
                                            INotificationHandler<ApplicationReviewedEvent>,
                                            INotificationHandler<SetupEmployerEvent>
    {
        
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<UpdateEmployerDashboardOnChange> _logger;
        

        public UpdateEmployerDashboardOnChange(IDashboardService dashboardService, ILogger<UpdateEmployerDashboardOnChange> logger)
        {
            _dashboardService = dashboardService;
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

        public Task Handle(SetupEmployerEvent notification, CancellationToken cancellationToken)
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

        private Task Handle(IApplicationReviewEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");
            
            _logger.LogInformation("Handling {eventType} for accountId: {employerAccountId} and vacancyId: {vacancyId}", notification.GetType().Name, notification.EmployerAccountId, notification.VacancyId);
            return _dashboardService.ReBuildDashboardAsync(notification.EmployerAccountId);
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
