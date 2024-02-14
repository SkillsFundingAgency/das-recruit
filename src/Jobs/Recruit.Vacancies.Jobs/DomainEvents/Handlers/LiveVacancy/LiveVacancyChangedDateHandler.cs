using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.LiveVacancy;

public class LiveVacancyChangedDateHandler : DomainEventHandler, IDomainEventHandler<LiveVacancyUpdatedEvent>
{
    private readonly ILogger<LiveVacancyChangedDateHandler> _logger;
    private readonly IMessageSession _messageSession;

    public LiveVacancyChangedDateHandler(ILogger<LiveVacancyChangedDateHandler> logger, IMessageSession messageSession) : base(logger)
    {
        _logger = logger;
        _messageSession = messageSession;
    }

    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<LiveVacancyUpdatedEvent>(eventPayload);

        try
        {
            _logger.LogInformation($"Processing {nameof(LiveVacancyUpdatedEvent)} to publish to NServiceBus");

            await _messageSession.Publish(@event);

            _logger.LogInformation($"Finished processing {nameof(LiveVacancyUpdatedEvent)} to publish to NServiceBus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process {eventBody}", @event);
            throw;
        }
    }
}