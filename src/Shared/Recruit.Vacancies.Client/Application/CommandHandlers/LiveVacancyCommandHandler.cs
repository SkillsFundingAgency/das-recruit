using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class LiveVacancyCommandHandler : IRequestHandler<LiveVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public LiveVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging, ITimeProvider timeProvider)
        {
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(LiveVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (!vacancy.CanMakeLive)
            {
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
