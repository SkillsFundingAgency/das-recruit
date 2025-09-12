using System;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class ApplicationSubmittedDomainEventHandler : DomainEventHandler, IDomainEventHandler<ApplicationSubmittedEvent>
    {
        private readonly ILogger<ApplicationSubmittedDomainEventHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly ICommunicationQueueService _communicationQueueService;
        private readonly IOuterApiClient _outerApiClient;
        private readonly IFeature _feature;

        public ApplicationSubmittedDomainEventHandler(
            ILogger<ApplicationSubmittedDomainEventHandler> logger,
            IJobsVacancyClient client,
            ICommunicationQueueService communicationQueueService,
            IOuterApiClient outerApiClient,
            IFeature feature)
            : base(logger)
        {
            _logger = logger;
            _client = client;
            _communicationQueueService = communicationQueueService;
            _outerApiClient = outerApiClient;
            _feature = feature;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationSubmittedEvent>(eventPayload);

            var vacancyReference = @event.Application.VacancyReference;
            var applicationId = @event.Application.ApplicationId;
            var candidateId = @event.Application.CandidateId;
            var vacancyId = @event.VacancyId;

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyId}} and candidate: {{CandidateId}}", vacancyId, candidateId);

                if (_feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
                {
                    await _outerApiClient.Post(new PostApplicationSubmittedEventRequest(new PostApplicationSubmittedEventData(applicationId, vacancyId)));
                }
                else 
                {
                    var communicationRequest = GetApplicationSubmittedCommunicationRequest(vacancyReference);

                    await _communicationQueueService.AddMessageAsync(communicationRequest);                    
                }
                
                await _client.CreateApplicationReviewAsync(@event.Application);

                _logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", vacancyReference, candidateId);
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

