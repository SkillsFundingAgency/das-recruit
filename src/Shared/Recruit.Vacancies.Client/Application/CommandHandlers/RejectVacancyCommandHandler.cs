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
    public class RejectVacancyCommandHandler : IRequestHandler<RejectVacancyCommand>
    {
        private readonly ILogger<RejectVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;

        public RejectVacancyCommandHandler(
            ILogger<RejectVacancyCommandHandler> logger,
            IVacancyRepository repository,
            IMessaging messaging)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
        }

        public async Task Handle(RejectVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rejecting vacancy {vacancyReference}.", message.VacancyReference);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanReview)
            {
                _logger.LogWarning($"Unable to refer vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
                return;
            }

            vacancy.Status = VacancyStatus.Rejected;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyRejectedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
        }
    }
}
