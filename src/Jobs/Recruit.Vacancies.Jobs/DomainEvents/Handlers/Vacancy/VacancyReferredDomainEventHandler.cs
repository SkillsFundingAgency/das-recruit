using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyReferredDomainEventHandler : DomainEventHandler, IDomainEventHandler<VacancyReferredEvent>
    {
        private readonly ILogger<VacancyReferredDomainEventHandler> _logger;
        private readonly ICommunicationQueueService _queue;
        private readonly IFeature _feature;

        public VacancyReferredDomainEventHandler(ILogger<VacancyReferredDomainEventHandler> logger, ICommunicationQueueService queue, IFeature feature) : base(logger)
        {
            _logger = logger;
            _queue = queue;
            _feature = feature;
        }

        public async Task HandleAsync(string eventPayload)
        {
            if (_feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            {
                return;
            }
            
            var @event = DeserializeEvent<VacancyReferredEvent>(eventPayload);
            var communicationRequest = GetReferredVacancyCommunicationRequest(@event.VacancyReference);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReferredEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _queue.AddMessageAsync(communicationRequest);

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
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.VacancyRejected, 
                CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName, 
                CommunicationConstants.ServiceName);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            return communicationRequest;
        }
    }
}

