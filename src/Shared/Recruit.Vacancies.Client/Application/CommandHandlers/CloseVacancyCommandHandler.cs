using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseVacancyCommandHandler : IRequestHandler<CloseVacancyCommand>
    {
        private readonly ILogger<CloseVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public CloseVacancyCommandHandler(ILogger<CloseVacancyCommandHandler> logger, IVacancyRepository vacancyRepository, ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Closing vacancy {vacancyId} with reason {closureReason}.", message.VacancyId, message.ClosureReason);

            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);

            //When expired vacancies are being closed automatically, there is no user to associate.
            if(message.User != null) vacancy.ClosedByUser = message.User;
            vacancy.ClosureReason = message.ClosureReason;

            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
            
        }
    }
}
