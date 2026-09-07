using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class UpdateLiveVacancyCommandHandler(
    ILogger<UpdateLiveVacancyCommandHandler> logger,
    IVacancyRepository repository,
    IMessaging messaging,
    ITimeProvider timeProvider)
    : IRequestHandler<UpdateLiveVacancyCommand, Unit>
{
    public async Task<Unit> Handle(UpdateLiveVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating live vacancy {vacancyId}.", message.Vacancy.Id);

        message.Vacancy.LastUpdatedDate = timeProvider.Now;

        await repository.UpdateAsync(message.Vacancy);

        if (message.UpdateKind.HasFlag(LiveUpdateKind.ClosingDate))
        {
            await PublishLiveVacancyClosingDateChangedEventAsync(message);
        }

        await messaging.PublishEvent(new VacancyPublishedEvent
        {
            VacancyId = message.Vacancy.Id
        });

        var liveVacancyUpdatedEvent = new LiveVacancyUpdatedEvent
        {
            VacancyId = message.Vacancy.Id,
            VacancyReference = message.Vacancy.VacancyReference.Value,
            UpdateKind = message.UpdateKind
        };
        await messaging.PublishEvent(liveVacancyUpdatedEvent);
            
        return Unit.Value;
    }

    private async Task PublishLiveVacancyClosingDateChangedEventAsync(UpdateLiveVacancyCommand message)
    {
        var vacancyId = message.Vacancy.Id;
        var closingDate = message.Vacancy.ClosingDate.Value;
        logger.LogInformation("Changing vacancy {vacancyId} closing date to {closingDate}.", message.Vacancy.Id, closingDate);

        await messaging.PublishEvent(new LiveVacancyClosingDateChangedEvent
        {
            VacancyId = vacancyId,
            VacancyReference = message.Vacancy.VacancyReference.Value,
            NewClosingDate = closingDate
        });
    }
}