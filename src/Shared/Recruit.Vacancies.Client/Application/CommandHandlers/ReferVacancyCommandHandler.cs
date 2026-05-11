using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyCommandHandler(
        ILogger<ReferVacancyCommandHandler> logger,
        IVacancyRepository repository,
        IMessageSession messageSession) : IRequestHandler<ReferVacancyCommand, Unit>
    {
        public async Task<Unit> Handle(ReferVacancyCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Referring vacancy {vacancyReference}.", message.VacancyReference);

            var vacancy = await repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanRefer)
            {
                logger.LogWarning($"Unable to refer vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
                return Unit.Value;
            }

            vacancy.Status = VacancyStatus.Referred;

            await repository.UpdateAsync(vacancy);
            
            var vacancyReferredEvent = new VacancyReferredEvent
            {
                VacancyReference = vacancy.VacancyReference!.Value,
                VacancyId = vacancy.Id
            };
            await messageSession.Publish(vacancyReferredEvent);
            return Unit.Value;
        }
    }
}
