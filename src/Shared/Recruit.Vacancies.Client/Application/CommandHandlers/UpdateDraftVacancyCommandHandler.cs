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

public class UpdateDraftVacancyCommandHandler(
    ILogger<UpdateDraftVacancyCommandHandler> logger,
    IVacancyRepository repository,
    IMessaging messaging,
    ITimeProvider timeProvider)
    : IRequestHandler<UpdateDraftVacancyCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDraftVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating vacancy {vacancyId}.", message.Vacancy.Id);

        message.Vacancy.LastUpdatedDate = timeProvider.Now;

        await repository.UpdateAsync(message.Vacancy);

        await messaging.PublishEvent(new DraftVacancyUpdatedEvent
        {
            EmployerAccountId = message.Vacancy.EmployerAccountId,
            VacancyId = message.Vacancy.Id
        });
            
        return Unit.Value;
    }
}