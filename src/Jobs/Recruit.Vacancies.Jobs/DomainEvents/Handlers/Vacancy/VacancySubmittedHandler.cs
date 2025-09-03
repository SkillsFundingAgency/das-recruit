using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancySubmittedHandler(
        ILogger<VacancySubmittedHandler> logger,
        IJobsVacancyClient client,
        IOuterApiClient outerApiClient)
        : DomainEventHandler(logger), IDomainEventHandler<VacancySubmittedEvent>
    {
        private const string EventName = nameof(VacancySubmittedEvent);
        
        public async Task HandleAsync(string eventPayload)
        {
            var vacancySubmittedEvent = DeserializeEvent<VacancySubmittedEvent>(eventPayload);
            try
            {
                logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", EventName, vacancySubmittedEvent.VacancyId);
                
                await client.CreateVacancyReview(vacancySubmittedEvent.VacancyReference);
                await client.EnsureVacancyIsGeocodedAsync(vacancySubmittedEvent.VacancyId);
                await outerApiClient.Post(new PostVacancySubmittedEventRequest(new PostVacancySubmittedEventData(vacancySubmittedEvent.VacancyId, vacancySubmittedEvent.VacancyReference)));
                
                logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", EventName, vacancySubmittedEvent.VacancyId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process {eventBody}", vacancySubmittedEvent);
                throw;
            }
        }
    }
}