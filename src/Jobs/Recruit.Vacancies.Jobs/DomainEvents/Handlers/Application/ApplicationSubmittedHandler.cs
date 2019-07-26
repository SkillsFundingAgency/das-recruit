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
    public class ApplicationSubmittedHandler : DomainEventHandler, IDomainEventHandler<ApplicationSubmittedEvent>
    {
        private readonly ILogger<ApplicationSubmittedHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly ICommunicationQueueService _communicationQueueService;
        public ApplicationSubmittedHandler(
            ILogger<ApplicationSubmittedHandler> logger,
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
            var commsRequest = GetReferredVacancyCommunicationRequest(@event.Application.VacancyReference);

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);

                await _client.CreateApplicationReviewAsync(@event.Application);

                await _communicationQueueService.AddMessageAsync(commsRequest);

                _logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private CommunicationRequest GetReferredVacancyCommunicationRequest(long vacancyReference)
        {
            var commsRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.ApplicationSubmitted, CommunicationConstants.ServiceName, CommunicationConstants.ServiceName);
            commsRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            commsRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            return commsRequest;
        }

    }
}

