using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloseVacancyCommandHandler : IRequestHandler<CloseVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CloseVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging, ITimeProvider timeProvider)
        {
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CloseVacancyCommand message, CancellationToken cancellationToken)
        {
            message.Vacancy.ClosedDate = _timeProvider.Now;
            message.Vacancy.Status = VacancyStatus.Closed;

            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.Vacancy.EmployerAccountId,
                VacancyReference = message.Vacancy.VacancyReference.Value,
                VacancyId = message.Vacancy.Id
            });
        }
    }
}
