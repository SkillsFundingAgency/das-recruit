using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;

public class VacancyRejectedHandler(
    ILogger<VacancyRejectedHandler> logger,
    IOuterApiClient outerApiClient)
    : DomainEventHandler(logger), IDomainEventHandler<VacancyRejectedEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<VacancyRejectedEvent>(eventPayload);
        try
        {
            logger.LogInformation("Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyRejectedEvent), @event.VacancyId);
            await outerApiClient.Post(new PostEmployerRejectedVacancyEventRequest(new PostEmployerRejectedVacancyEventData(@event.VacancyId)));
            logger.LogInformation("Finished Processing {EventName} for vacancy: {VacancyId}", nameof(VacancyRejectedEvent), @event.VacancyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }
}