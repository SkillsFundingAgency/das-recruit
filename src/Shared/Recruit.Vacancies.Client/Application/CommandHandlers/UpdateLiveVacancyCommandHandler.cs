using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateLiveVacancyCommandHandler : IRequestHandler<UpdateLiveVacancyCommand, Unit>
    {
        private readonly ILogger<UpdateLiveVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessageSession _messageSession;

        public UpdateLiveVacancyCommandHandler(
            ILogger<UpdateLiveVacancyCommandHandler> logger,
            IVacancyRepository repository,
            IMessaging messaging,
            ITimeProvider timeProvider, 
            IMessageSession messageSession)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _messageSession = messageSession;
        }

        public async Task<Unit> Handle(UpdateLiveVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating live vacancy {vacancyId}.", message.Vacancy.Id);

            message.Vacancy.LastUpdatedDate = _timeProvider.Now;
            message.Vacancy.LastUpdatedByUser = message.User;

            await _repository.UpdateAsync(message.Vacancy);

            if (message.UpdateKind.HasFlag(LiveUpdateKind.ClosingDate))
            {
                await PublishLiveVacancyClosingDateChangedEventAsync(message);
            }

            await _messaging.PublishEvent(new VacancyPublishedEvent
            {
                VacancyId = message.Vacancy.Id
            });

            var liveVacancyUpdatedEvent = new LiveVacancyUpdatedEvent
            {
                VacancyId = message.Vacancy.Id,
                VacancyReference = message.Vacancy.VacancyReference.Value,
                UpdateKind = message.UpdateKind
            };
            await Task.WhenAll(_messaging.PublishEvent(liveVacancyUpdatedEvent), _messageSession.Publish(liveVacancyUpdatedEvent));
            
            return Unit.Value;
        }

        private async Task PublishLiveVacancyClosingDateChangedEventAsync(UpdateLiveVacancyCommand message)
        {
            var vacancyId = message.Vacancy.Id;
            var closingDate = message.Vacancy.ClosingDate.Value;
            _logger.LogInformation("Changing vacancy {vacancyId} closing date to {closingDate}.", message.Vacancy.Id, closingDate);

            await _messaging.PublishEvent(new LiveVacancyClosingDateChangedEvent
            {
                VacancyId = vacancyId,
                VacancyReference = message.Vacancy.VacancyReference.Value,
                NewClosingDate = closingDate
            });
        }
    }
}
