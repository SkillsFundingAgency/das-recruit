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
    public class PublishVacancyCommandHandler : IRequestHandler<PublishVacancyCommand>
    {
        private readonly ILogger<PublishVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public PublishVacancyCommandHandler(
            ILogger<PublishVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(PublishVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting vacancy Live for vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (!vacancy.CanMakeLive)
            {
                _logger.LogWarning($"Cannot make vacancy {{vacancyId}} Live due to vacancy having a status of {vacancy.Status}.", vacancy.Id);
                return;
            }

            vacancy.Status = VacancyStatus.Live;
            vacancy.LiveDate = _timeProvider.Now;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyLiveEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }
    }
}
