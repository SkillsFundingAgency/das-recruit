using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyCommandHandler : IRequestHandler<ReferVacancyCommand, Unit>
    {
        private readonly ILogger<ReferVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;

        public ReferVacancyCommandHandler(
            ILogger<ReferVacancyCommandHandler> logger,
            IVacancyRepository repository,
            IMessaging messaging)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
        }

        public async Task<Unit> Handle(ReferVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Referring vacancy {vacancyReference}.", message.VacancyReference);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanRefer)
            {
                _logger.LogWarning($"Unable to refer vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
                return Unit.Value;
            }

            vacancy.Status = VacancyStatus.Referred;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyReferredEvent
            {
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
            return Unit.Value;
        }
    }
}
