using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class PublishVacancyCommandHandler(
        ILogger<PublishVacancyCommandHandler> logger,
        IVacancyRepository repository,
        IMessaging messaging,
        ITimeProvider timeProvider)
        : IRequestHandler<PublishVacancyCommand, Unit>
    {
        public async Task<Unit> Handle(PublishVacancyCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Setting vacancy Live for vacancy {vacancyId}.", message.VacancyId);

            await messaging.SendCommandAsync(new GeocodeVacancyCommand { VacancyId = message.VacancyId });

            var vacancy = await repository.GetVacancyAsync(message.VacancyId);
            if (!vacancy.CanMakeLive)
            {
                logger.LogWarning("Cannot make vacancy {vacancyId} Live due to vacancy having a status of {vacancy.Status}.", message.VacancyId, vacancy.Id);
                return Unit.Value;
            }

            vacancy.Status = VacancyStatus.Live;
            vacancy.LiveDate = timeProvider.Now;
            await repository.UpdateAsync(vacancy);
            await messaging.PublishEvent(new VacancyPublishedEvent { VacancyId = vacancy.Id });
            return Unit.Value;
        }
    }
}
