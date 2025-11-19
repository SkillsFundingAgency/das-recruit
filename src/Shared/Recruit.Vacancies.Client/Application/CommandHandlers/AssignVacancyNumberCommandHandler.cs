using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class AssignVacancyNumberCommandHandler(
    IVacancyRepository repository,
    IMessaging messaging,
    ILogger<AssignVacancyNumberCommandHandler> logger,
    IOuterApiVacancyClient recruitOuterClient)
    : IRequestHandler<AssignVacancyNumberCommand, Unit>
{
    public async Task<Unit> Handle(AssignVacancyNumberCommand message, CancellationToken cancellationToken)
    {
        var vacancy = await repository.GetVacancyAsync(message.VacancyId);
        if (vacancy.VacancyReference.HasValue)
        {
            logger.LogInformation("Vacancy '{VacancyId}' already has a vacancy reference ({VacancyReference}).", vacancy.Id, vacancy.VacancyReference);
            return Unit.Value;
        }

        vacancy.VacancyReference = await recruitOuterClient.GetNextVacancyIdAsync();

        logger.LogInformation("Assigning reference '{VacancyReference}' to vacancy '{VacancyId}'.", vacancy.VacancyReference, message.VacancyId);
        await repository.UpdateAsync(vacancy);
        await messaging.PublishEvent(new DraftVacancyUpdatedEvent
        {
            EmployerAccountId = vacancy.EmployerAccountId,
            VacancyId = vacancy.Id
        });
        return Unit.Value;
    }
}