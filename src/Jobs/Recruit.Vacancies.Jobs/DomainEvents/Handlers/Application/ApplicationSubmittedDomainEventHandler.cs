using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;

public class ApplicationSubmittedDomainEventHandler(
    ILogger<ApplicationSubmittedDomainEventHandler> logger,
    IJobsVacancyClient client)
    : DomainEventHandler(logger), IDomainEventHandler<ApplicationSubmittedEvent>
{
    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<ApplicationSubmittedEvent>(eventPayload);
        var vacancyReference = @event.Application.VacancyReference;
        var candidateId = @event.Application.CandidateId;
        var vacancyId = @event.VacancyId;

        try
        {
            logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyId}} and candidate: {{CandidateId}}", vacancyId, candidateId);
            await client.CreateApplicationReviewAsync(@event.Application);
            logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", vacancyReference, candidateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process {EventBody}", @event);
            throw;
        }
    }
}