using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateVacancyApplicationsOnApplicationReviewChange :
        INotificationHandler<ApplicationReviewCreatedEvent>,
        INotificationHandler<ApplicationReviewWithdrawnEvent>,
        INotificationHandler<ApplicationReviewDeletedEvent>,
        INotificationHandler<ApplicationReviewedEvent>
    {
        private readonly IVacancyApplicationsProjectionService _projectionService;
        private readonly ILogger<UpdateVacancyApplicationsOnApplicationReviewChange> _logger;

        public UpdateVacancyApplicationsOnApplicationReviewChange(ILogger<UpdateVacancyApplicationsOnApplicationReviewChange> logger, IVacancyApplicationsProjectionService projectionService)
        {
            _projectionService = projectionService;
            _logger = logger;
        }

        public Task Handle(ApplicationReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewWithdrawnEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewDeletedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private Task Handle(IApplicationReviewEvent notification)
        {
            _logger.LogInformation("Handling {notificationType} for vacancyReference: {vacancyReference}", notification.GetType().Name, notification.VacancyReference);

            return _projectionService.UpdateVacancyApplicationsAsync(notification.VacancyReference);
        }
    }
}
