using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateVacancyCommandHandler : IRequestHandler<UpdateVacancyCommand>
    {
        private readonly ILogger<UpdateVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public UpdateVacancyCommandHandler(
            ILogger<UpdateVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(UpdateVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating vacancy {vacancyId}.", message.Vacancy.Id);

            message.Vacancy.LastUpdatedDate = _timeProvider.Now;
            message.Vacancy.LastUpdatedByUser = message.User;

            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancyDraftUpdatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.Vacancy.EmployerAccountId,
                VacancyId = message.Vacancy.Id
            });
        }
    }
}
