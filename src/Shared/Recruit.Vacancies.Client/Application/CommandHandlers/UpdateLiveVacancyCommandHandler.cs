using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateLiveVacancyCommandHandler : IRequestHandler<UpdateLiveVacancyCommand>
    {
        private readonly ILogger<UpdateLiveVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public UpdateLiveVacancyCommandHandler(
            ILogger<UpdateLiveVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(UpdateLiveVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating live vacancy {vacancyId}.", message.Vacancy.Id);

            Vacancy originalVacancy = await _repository.GetVacancyAsync(message.Vacancy.Id);
            await PublishLiveVacancyClosingDateChangedEvent(originalVacancy, message.Vacancy.ClosingDate);

            message.Vacancy.LastUpdatedDate = _timeProvider.Now;
            message.Vacancy.LastUpdatedByUser = message.User;

            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancyPublishedEvent
            {
                VacancyId = message.Vacancy.Id
            });
        }

        private async Task PublishLiveVacancyClosingDateChangedEvent(Vacancy originalVacancy, DateTime? newClosingDate)
        {
            bool shouldPublishEvent =
                originalVacancy.Status == VacancyStatus.Live
                && newClosingDate.HasValue
                && newClosingDate != originalVacancy.ClosingDate;

            if (shouldPublishEvent)
            {
                _logger.LogInformation("Changing vacancy {vacancyId} closing date to {ClosingDate}.", originalVacancy.Id, newClosingDate.Value);

                var @event = new LiveVacancyClosingDateChangedEvent(
                    originalVacancy.Id, originalVacancy.VacancyReference.Value, newClosingDate.Value);
                await _messaging.PublishEvent(@event);
            }
        }
    }
}
