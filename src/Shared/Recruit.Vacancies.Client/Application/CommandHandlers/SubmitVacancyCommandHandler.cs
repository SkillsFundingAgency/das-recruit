using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SubmitVacancyCommandHandler : IRequestHandler<SubmitVacancyCommand>
    {
        private readonly ILogger<SubmitVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public SubmitVacancyCommandHandler(
            ILogger<SubmitVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(SubmitVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Submitting vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);
            
            if (vacancy == null || vacancy.CanSubmit == false)
            {
                _logger.LogWarning($"Unable to submit vacancy {{vacancyId}} due to review having a status of {vacancy?.Status}.", message.VacancyId);
                return;
            }
            
            var now = _timeProvider.Now;

            vacancy.Status = VacancyStatus.Submitted;
            vacancy.SubmittedDate = now;
            vacancy.SubmittedByUser = message.User;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = message.User;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancySubmittedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value
            });
        }
    }
}
