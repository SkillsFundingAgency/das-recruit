using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services;

public class VacancyService(
    ILogger<VacancyService> logger,
    IVacancyRepository vacancyRepository,
    ITimeProvider timeProvider,
    IMessaging messaging)
    : IVacancyService
{
    public async Task CloseExpiredVacancy(Guid vacancyId)
    {
        logger.LogInformation("Closing vacancy {vacancyId}.", vacancyId);

        var vacancy = await vacancyRepository.GetVacancyAsync(vacancyId);
        vacancy.ClosureReason = ClosureReason.Auto;

        vacancy.ClosedDate = timeProvider.Now;
        vacancy.Status = VacancyStatus.Closed;

        await vacancyRepository.UpdateAsync(vacancy);

        await messaging.PublishEvent(new VacancyClosedEvent
        {
            VacancyReference = vacancy.VacancyReference.Value,
            VacancyId = vacancy.Id
        });
    }
}