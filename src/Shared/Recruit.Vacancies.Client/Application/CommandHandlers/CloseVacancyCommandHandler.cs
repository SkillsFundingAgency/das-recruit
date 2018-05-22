using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseVacancyCommandHandler : IRequestHandler<CloseVacancyCommand>
    {
        private readonly ILogger<CloseVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CloseVacancyCommandHandler(
            ILogger<CloseVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Closing vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
        }
    }
}
