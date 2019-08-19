using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class NotifyOnVacancyActionsEventHandler : INotificationHandler<VacancyClosedEvent>,
                                                        INotificationHandler<LiveVacancyUpdatedEvent>
    {
        private readonly INotifyVacancyUpdates _vacancyStatusNotifier;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<NotifyOnVacancyActionsEventHandler> _logger;
        private readonly ICommunicationQueueService _communicationQueueService;

        public NotifyOnVacancyActionsEventHandler(
            ILogger<NotifyOnVacancyActionsEventHandler> logger, 
            INotifyVacancyUpdates vacancyStatusNotifier, 
            IVacancyRepository vacancyRepository, 
            ICommunicationQueueService communicationQueueService)
        {
            _vacancyStatusNotifier = vacancyStatusNotifier;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
            _communicationQueueService = communicationQueueService;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);

            if (vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
            {
                var commsRequest = GetVacancyWithdrawnByQaCommunicationRequest(vacancy.VacancyReference.Value);
                await _communicationQueueService.AddMessageAsync(commsRequest);
            }

            try
            {
                if (vacancy.ClosureReason == ClosureReason.Manual)
                {
                    await _vacancyStatusNotifier.VacancyManuallyClosed(vacancy);
                }
            }
            catch (NotificationException ex)
            {
                _logger.LogError(ex, $"Unable to send notifications for {nameof(VacancyClosedEvent)} and VacancyReference: {{vacancyReference}}", notification.VacancyReference);
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

        private CommunicationRequest GetVacancyWithdrawnByQaCommunicationRequest(long vacancyReference)
        {
            var commsRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.VacancyWithdrawnByQa, CommunicationConstants.ServiceName, CommunicationConstants.ServiceName);
            commsRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            return commsRequest;
        }
    }
}