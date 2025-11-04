using System;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;

public class VacancyReviewedHandler(
    ILogger<VacancyReviewedHandler> logger,
    ICommunicationQueueService queue,
    IOuterApiClient outerApiClient,
    IFeature feature)
    : DomainEventHandler(logger), IDomainEventHandler<VacancyReviewedEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<VacancyReviewedEvent>(eventPayload);
            
        try
        {
            logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyReviewedEvent), @event.VacancyId);
                
            if (feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            {
                await outerApiClient.Post(new PostVacancySubmittedEventRequest(new PostVacancySubmittedEventData(@event.VacancyId, @event.VacancyReference)));
            }
            else
            {
                var communicationRequest = GetReviewedVacancyCommunicationRequest(@event.VacancyReference, @event.Ukprn, @event.EmployerAccountId);
                await queue.AddMessageAsync(communicationRequest);                    
            }
                
            logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyReviewedEvent), @event.VacancyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }

    private CommunicationRequest GetReviewedVacancyCommunicationRequest(long vacancyReference, long ukprn, string employerAccountId)
    {
        var communicationRequest = new CommunicationRequest(
            CommunicationConstants.RequestType.VacancySubmittedForReview,
            CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName,
            CommunicationConstants.ServiceName);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Employer, employerAccountId);
        return communicationRequest;
    }
}