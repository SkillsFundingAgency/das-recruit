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

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseVacancyCommandHandler : IRequestHandler<CloseVacancyCommand, Unit>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<CloseVacancyCommandHandler> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        public CloseVacancyCommandHandler(
            IVacancyRepository vacancyRepository,
            ITimeProvider timeProvider,
            IMessaging messaging,
            ILogger<CloseVacancyCommandHandler> logger)
        {
            _vacancyRepository = vacancyRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
            _logger = logger;
        }

        public async Task<Unit> Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);

            if (vacancy == null || vacancy.Status != VacancyStatus.Live)
            {
                _logger.LogInformation($"Cannot close vacancy {message.VacancyId} as it was not found or is not in status live.");
            }

            _logger.LogInformation("Closing vacancy {vacancyId} by user {userEmail}.", vacancy.Id, message.User.Email);
            vacancy.ClosedByUser = message.User;
            vacancy.ClosureReason = message.ClosureReason;

            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                VacancyReference = vacancy.VacancyReference.GetValueOrDefault(),
                VacancyId = vacancy.Id
            });
            return Unit.Value;
        }

    }
}
