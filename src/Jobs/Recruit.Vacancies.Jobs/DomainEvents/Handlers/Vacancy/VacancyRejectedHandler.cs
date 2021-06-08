using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyRejectedHandler : DomainEventHandler, IDomainEventHandler<VacancyRejectedEvent>
    {
        private readonly ILogger<VacancyRejectedHandler> _logger;
        private readonly ICommunicationQueueService _queue;

        public VacancyRejectedHandler(ILogger<VacancyRejectedHandler> logger, ICommunicationQueueService queue) : base(logger)
        {
            _logger = logger;
            _queue = queue;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyRejectedEvent>(eventPayload);
            var communicationRequest = GetRejectedVacancyCommunicationRequest(@event.VacancyReference, @event.ProviderUkprn);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyRejectedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _queue.AddMessageAsync(communicationRequest);

                _logger.LogInformation($"Finished Processing {nameof(VacancyRejectedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private CommunicationRequest GetRejectedVacancyCommunicationRequest(long vacancyReference, long? providerUkprn)
        {
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.VacancyRejectedByEmployer,
                CommunicationConstants.ParticipantResolverNames.ProviderParticipantsResolverName,
                CommunicationConstants.ServiceName);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, providerUkprn);
            return communicationRequest;
        }
    }
}
