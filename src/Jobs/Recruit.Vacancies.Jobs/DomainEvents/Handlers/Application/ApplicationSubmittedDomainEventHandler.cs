using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
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
        private readonly IFeature _feature;

        public ApplicationSubmittedDomainEventHandler(
            ILogger<ApplicationSubmittedDomainEventHandler> logger,
            IJobsVacancyClient client,
            ICommunicationQueueService communicationQueueService,
            IFeature feature)
            : base(logger)
        {
            _logger = logger;
            _client = client;
            _communicationQueueService = communicationQueueService;
            _feature = feature;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationSubmittedEvent>(eventPayload);
            var vacancyReference = @event.Application.VacancyReference;
            var candidateId = @event.Application.CandidateId;
            var vacancyId = @event.VacancyId;

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyId}} and candidate: {{CandidateId}}", vacancyId, candidateId);
                await _client.CreateApplicationReviewAsync(@event.Application);
                _logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", vacancyReference, candidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

