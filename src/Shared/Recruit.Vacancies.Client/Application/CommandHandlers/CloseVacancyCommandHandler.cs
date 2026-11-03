using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class CloseVacancyCommandHandler(
    IVacancyRepository vacancyRepository,
    ITimeProvider timeProvider,
    IMessaging messaging,
    ILogger<CloseVacancyCommandHandler> logger)
    : IRequestHandler<CloseVacancyCommand, Unit>
{
    public async Task<Unit> Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
    {
        var vacancy = await vacancyRepository.GetVacancyAsync(message.VacancyId);

        if (vacancy == null || vacancy.Status != VacancyStatus.Live)
        {
            logger.LogInformation($"Cannot close vacancy {message.VacancyId} as it was not found or is not in status live.");
        }

        logger.LogInformation("Closing vacancy {vacancyId} by user {userEmail}.", vacancy.Id, message.User.Email);
        vacancy.ClosureReason = message.ClosureReason;
        vacancy.ClosedDate = timeProvider.Now;
        vacancy.Status = VacancyStatus.Closed;

        await vacancyRepository.UpdateAsync(vacancy);

        await messaging.PublishEvent(new VacancyClosedEvent
        {
            VacancyReference = vacancy.VacancyReference.GetValueOrDefault(),
            VacancyId = vacancy.Id
        });
        return Unit.Value;
    }

}