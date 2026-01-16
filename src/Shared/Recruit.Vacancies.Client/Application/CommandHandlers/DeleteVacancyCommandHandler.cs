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

public class DeleteVacancyCommandHandler(
    ILogger<DeleteVacancyCommandHandler> logger,
    IVacancyRepository repository,
    IMessaging messaging,
    ITimeProvider timeProvider)
    : IRequestHandler<DeleteVacancyCommand, Unit>
{
    public async Task<Unit> Handle(DeleteVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting vacancy {vacancyId}", message.VacancyId);
            
        var vacancy = await repository.GetVacancyAsync(message.VacancyId);

        if (vacancy == null)
        {
            logger.LogWarning($"Unable to find vacancy {{vacancyId}} for deletion", message.VacancyId);
            return Unit.Value;
        }

        if (vacancy.CanDelete == false)
        {
            logger.LogWarning($"Unable to delete vacancy {{vacancyId}} due to vacancy having a status of {vacancy?.Status}.", message.VacancyId);
            return Unit.Value;
        }

        var now = timeProvider.Now;

        vacancy.IsDeleted = true;
        vacancy.DeletedDate = now;
        vacancy.LastUpdatedDate = now;

        await repository.UpdateAsync(vacancy);

        await messaging.PublishEvent(new VacancyDeletedEvent
        {
            VacancyId = vacancy.Id
        });
            
        return Unit.Value;
    }
}