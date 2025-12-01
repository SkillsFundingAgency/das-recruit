using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;

public class ApplicationWithdrawnHandler(ILogger<ApplicationWithdrawnEvent> logger, IJobsVacancyClient client)
    : DomainEventHandler(logger), IDomainEventHandler<ApplicationWithdrawnEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<ApplicationWithdrawnEvent>(eventPayload);
        try
        {
            logger.LogInformation($"Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);
            await client.WithdrawApplicationAsync(@event.VacancyReference, @event.CandidateId);
            logger.LogInformation($"Finished Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }
}