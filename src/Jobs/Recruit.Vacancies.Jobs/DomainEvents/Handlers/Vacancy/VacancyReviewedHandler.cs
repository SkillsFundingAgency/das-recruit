using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;

public class VacancyReviewedHandler(
    ILogger<VacancyReviewedHandler> logger,
    IOuterApiClient outerApiClient)
    : DomainEventHandler(logger), IDomainEventHandler<VacancyReviewedEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<VacancyReviewedEvent>(eventPayload);
        try
        {
            logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyReviewedEvent), @event.VacancyId);
            await outerApiClient.Post(new PostVacancySubmittedEventRequest(new PostVacancySubmittedEventData(@event.VacancyId, @event.VacancyReference)));
            logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyReviewedEvent), @event.VacancyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }
}