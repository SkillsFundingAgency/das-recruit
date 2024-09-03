using System;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class ApplicationSubmittedDomainEventHandler : DomainEventHandler, IDomainEventHandler<ApplicationSubmittedEvent>
    {
        private readonly ILogger<ApplicationSubmittedDomainEventHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly ICommunicationQueueService _communicationQueueService;

        public ApplicationSubmittedDomainEventHandler(
            ILogger<ApplicationSubmittedDomainEventHandler> logger,
            IJobsVacancyClient client,
            ICommunicationQueueService communicationQueueService)
            : base(logger)
        {
            _logger = logger;
            _client = client;
            _communicationQueueService = communicationQueueService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationSubmittedEvent>(eventPayload);
            var communicationRequest = GetApplicationSubmittedCommunicationRequest(@event.Application.VacancyReference);

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);

                await _client.CreateApplicationReviewAsync(@event.Application);

                await _communicationQueueService.AddMessageAsync(communicationRequest);

                _logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private CommunicationRequest GetApplicationSubmittedCommunicationRequest(long vacancyReference)
        {
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.ApplicationSubmitted,
                CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName,
                CommunicationConstants.ServiceName);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUnsubscribeUrl, vacancyReference);
            return communicationRequest;
        }
    }
}

