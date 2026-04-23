using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class ArchiveVacancyCommandHandler(ILogger<ArchiveVacancyCommandHandler> logger,
    IVacancyRepository repository,
    ITimeProvider timeProvider)
    : IRequestHandler<ArchiveVacancyCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting vacancy {vacancyId}", message.VacancyId);

        var vacancy = await repository.GetVacancyAsync(message.VacancyId);

        if (vacancy == null)
        {
            logger.LogWarning("Unable to find vacancy {vacancyId} for archiving", message.VacancyId);
            return Unit.Value;
        }

        if (!vacancy.CanArchive)
        {
            logger.LogWarning("Unable to archive vacancy {vacancyId} due to vacancy having a status of {VacancyStatus}.", vacancy.Status, message.VacancyId);
            return Unit.Value;
        }

        var now = timeProvider.Now;

        vacancy.Status = VacancyStatus.Archived;
        vacancy.LastUpdatedDate = now;

        await repository.UpdateAsync(vacancy);

        return Unit.Value;
    }
}