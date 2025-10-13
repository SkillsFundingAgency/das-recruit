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

public class VacancyRejectedHandler(
    ILogger<VacancyRejectedHandler> logger,
    ICommunicationQueueService queue,
    IOuterApiClient outerApiClient,
    IFeature feature)
    : DomainEventHandler(logger), IDomainEventHandler<VacancyRejectedEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<VacancyRejectedEvent>(eventPayload);
        try
        {
            logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyRejectedEvent), @event.VacancyId);

            if (!feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
            {
                await outerApiClient.Post(new PostEmployerRejectedVacancyEventRequest(new PostEmployerRejectedVacancyEventData(@event.VacancyId)));
            }
            else
            {
                var communicationRequest = GetRejectedVacancyCommunicationRequest(@event.VacancyReference, @event.ProviderUkprn);
                await queue.AddMessageAsync(communicationRequest);
            }
            
            logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyRejectedEvent), @event.VacancyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }
    
    private static CommunicationRequest GetRejectedVacancyCommunicationRequest(long vacancyReference, long? providerUkprn)
    {
        var communicationRequest = new CommunicationRequest(
            CommunicationConstants.RequestType.VacancyRejectedByEmployer,
            CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName,
            CommunicationConstants.ServiceName);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
        communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, providerUkprn);
        return communicationRequest;
    }
}