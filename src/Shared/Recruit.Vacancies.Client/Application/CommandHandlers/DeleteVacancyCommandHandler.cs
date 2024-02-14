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
    public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand, Unit>
    {
        private readonly ILogger<DeleteVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessageSession _messageSession;

        public DeleteVacancyCommandHandler(
            ILogger<DeleteVacancyCommandHandler> logger,
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

        public async Task<Unit> Handle(DeleteVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting vacancy {vacancyId}", message.VacancyId);
            
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (vacancy == null)
            {
                _logger.LogWarning($"Unable to find vacancy {{vacancyId}} for deletion", message.VacancyId);
                return Unit.Value;
            }

            if (vacancy.CanDelete == false)
            {
                _logger.LogWarning($"Unable to delete vacancy {{vacancyId}} due to vacancy having a status of {vacancy?.Status}.", message.VacancyId);
                return Unit.Value;
            }

            var now = _timeProvider.Now;

            vacancy.IsDeleted = true;
            vacancy.DeletedDate = now;
            vacancy.LastUpdatedDate = now;

            if (message.User != null)
            {
                vacancy.DeletedByUser = message.User;
                vacancy.LastUpdatedByUser = message.User;
            }

            await _repository.UpdateAsync(vacancy);

            var vacancyDeletedEvent = new VacancyDeletedEvent
            {
                VacancyId = vacancy.Id
            };

            await Task.WhenAll(_messaging.PublishEvent(vacancyDeletedEvent),
                _messageSession.Publish(vacancyDeletedEvent));
            
            return Unit.Value;
        }
    }
}
