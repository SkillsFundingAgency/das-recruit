using System;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyReviewedHandler : DomainEventHandler, IDomainEventHandler<VacancyReviewedEvent>
    {
        private readonly ILogger<VacancyReviewedHandler> _logger;
        private readonly ICommunicationQueueService _queue;

        public VacancyReviewedHandler(ILogger<VacancyReviewedHandler> logger, ICommunicationQueueService queue) : base(logger)
        {
            _logger = logger;
            _queue = queue;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewedEvent>(eventPayload);
            
            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReviewedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                var communicationRequest = GetReviewedVacancyCommunicationRequest(@event.VacancyReference, @event.Ukprn, @event.EmployerAccountId);
                await _queue.AddMessageAsync(communicationRequest);

                _logger.LogInformation($"Finished Processing {nameof(VacancyReviewedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private CommunicationRequest GetReviewedVacancyCommunicationRequest(long vacancyReference, long ukprn, string employerAccountId)
        {
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.VacancySubmittedForReview,
                CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName,
                CommunicationConstants.ServiceName);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Employer, employerAccountId);
            return communicationRequest;
        }
    }
}

