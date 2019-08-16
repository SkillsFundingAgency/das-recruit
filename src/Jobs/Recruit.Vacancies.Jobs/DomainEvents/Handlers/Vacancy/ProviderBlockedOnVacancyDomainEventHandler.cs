using System.Threading.Tasks;
using Entities = Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class ProviderBlockedOnVacancyDomainEventHandler : DomainEventHandler, IDomainEventHandler<ProviderBlockedOnVacancyEvent>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;
        private readonly ILogger<ProviderBlockedOnVacancyDomainEventHandler> _logger;
        public ProviderBlockedOnVacancyDomainEventHandler(
            IVacancyRepository vacancyRepository,
            IMessaging messaging,
            ILogger<ProviderBlockedOnVacancyDomainEventHandler> logger) : base(logger)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<ProviderBlockedOnVacancyEvent>(eventPayload);

            _logger.LogInformation($"Updating vacancy {eventData.VacancyId} as the provider {eventData.Ukprn} is blocked");

            var vacancy = await _vacancyRepository.GetVacancyAsync(eventData.VacancyId);

            if (vacancy.OwnerType == Entities.OwnerType.Provider)
            {
                await _messaging.SendCommandAsync(
                    new TransferProviderVacancyCommand(
                        vacancy.Id,
                        eventData.QaVacancyUser,
                        eventData.BlockedDate,
                        Entities.TransferReason.BlockedByQa
                ));
            }

            if (vacancy.Status == Entities.VacancyStatus.Submitted)
            {
                await _messaging.SendCommandAsync(new ResetSubmittedVacancyCommand(vacancy.Id));
            }

            if (vacancy.Status == Entities.VacancyStatus.Live)
            {
                await _messaging.SendCommandAsync(new CloseVacancyCommand(vacancy.Id, eventData.QaVacancyUser, Entities.ClosureReason.BlockedByQa));
            }
        }
    }
}