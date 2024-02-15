using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.LiveVacancy;

public class LiveVacancyWithdrawnHandler : DomainEventHandler, IDomainEventHandler<VacancyClosedEvent>
{
    private readonly ILogger<LiveVacancyWithdrawnHandler> _logger;
    private readonly IMessageSession _messageSession;

    public LiveVacancyWithdrawnHandler(ILogger<LiveVacancyWithdrawnHandler> logger, IMessageSession messageSession) : base(logger)
    {
        _logger = logger;
        _messageSession = messageSession;
    }

    public async Task HandleAsync(string eventPayload)
    {
        var @event = DeserializeEvent<VacancyClosedEvent>(eventPayload);

        try
        {
            _logger.LogInformation($"Processing {nameof(VacancyClosedEvent)} to publish to NServiceBus");

            await _messageSession.Publish(@event);

            _logger.LogInformation($"Finished processing {nameof(VacancyClosedEvent)} to publish to NServiceBus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process {eventBody}", @event);
            throw;
        }
    }
}