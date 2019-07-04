using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyReferredDomainEventHandler : DomainEventHandler, IDomainEventHandler<VacancyReferredEvent>
    {
        private readonly ILogger<VacancyReferredDomainEventHandler> _logger;
        private readonly ICommunicationQueueService _queue;

        public VacancyReferredDomainEventHandler(ILogger<VacancyReferredDomainEventHandler> logger, ICommunicationQueueService queue) : base(logger)
        {
            _logger = logger;
            _queue = queue;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReferredEvent>(eventPayload);
            var commsRequest = GetReferredVacancyCommunicationRequest(@event.VacancyReference);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReferredEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _queue.AddMessageAsync(commsRequest);

                _logger.LogInformation($"Finished Processing {nameof(VacancyReferredEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private CommunicationRequest GetReferredVacancyCommunicationRequest(long vacancyReference)
        {
            var commsRequest = new CommunicationRequest(CommunicationConstants.RequestType.VacancyRejected, CommunicationConstants.ServiceName, CommunicationConstants.ServiceName);
            commsRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            return commsRequest;
        }
    }
}

