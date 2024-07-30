using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApproveVacancyCommandHandler : IRequestHandler<ApproveVacancyCommand, Unit>
    {
        private readonly ILogger<ApproveVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessageSession _messageSession;

        public ApproveVacancyCommandHandler(
                        ILogger<ApproveVacancyCommandHandler> logger, 
                        IVacancyRepository repository, 
                        IMessaging messaging, 
                        ITimeProvider timeprovider,
                        IMessageSession messageSession)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeprovider;
            _messageSession = messageSession;
        }

        public async Task<Unit> Handle(ApproveVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Approving vacancy {vacancyReference}.", message.VacancyReference);
            
            var vacancy = await _repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanApprove)
            {
                _logger.LogWarning($"Unable to approve vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
                return Unit.Value;
            }
            
            vacancy.Status = VacancyStatus.Approved;
            vacancy.ApprovedDate = _timeProvider.Now;
            
            await _repository.UpdateAsync(vacancy);

            var vacancyApprovedEvent = new VacancyApprovedEvent
            {
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            };
            await Task.WhenAll(_messaging.PublishEvent(vacancyApprovedEvent), _messageSession.Publish(vacancyApprovedEvent));
            return Unit.Value;
        }
    }
}
