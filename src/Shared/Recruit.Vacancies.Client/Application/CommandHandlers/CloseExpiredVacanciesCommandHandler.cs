using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class CloseExpiredVacanciesCommandHandler(
    ILogger<CloseExpiredVacanciesCommandHandler> logger,
    IVacancyQuery query,
    ITimeProvider timeProvider,
    IVacancyService vacancyService,
    IQueryStoreReader queryStoreReader,
    IMessaging messaging)
    : IRequestHandler<CloseExpiredVacanciesCommand, Unit>
{
    public async Task<Unit> Handle(CloseExpiredVacanciesCommand message, CancellationToken cancellationToken)
    {
        var vacancies = await query.GetVacanciesToCloseAsync(timeProvider.Today);
        var numberClosed = 0;
            
        foreach (var vacancyIdentity in vacancies)
        {
            logger.LogInformation("Closing vacancy {VacancyReference} with closing date of {ClosingDate}", vacancyIdentity.VacancyReference, vacancyIdentity.ClosingDate);
            await vacancyService.CloseExpiredVacancy(vacancyIdentity.Id);
            numberClosed++;
        }
            
        logger.LogInformation("Closed {closedCount} live vacancies", numberClosed);
        var orphanedVacancies = (await queryStoreReader.GetLiveExpiredVacancies(timeProvider.Today)).ToList();

        foreach (var orphanedVacancy in orphanedVacancies)
        {
            await messaging.PublishEvent(new VacancyClosedEvent
            {
                VacancyId = orphanedVacancy.VacancyId,
                VacancyReference = orphanedVacancy.VacancyReference
            });
        }
            
        logger.LogInformation("Closed {closedCount} orphaned live vacancies", orphanedVacancies.Count);
        return Unit.Value;
    }
}