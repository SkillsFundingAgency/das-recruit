using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyCommandHandler : IRequestHandler<ReferVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public ReferVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging, ITimeProvider timeprovider)
        {
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeprovider;
        }

        public async Task Handle(ReferVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanRefer)
            {
                return;
            }
            
            vacancy.Status = VacancyStatus.UnderReview; // This will be changed after private beta.
            
            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyUpdatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }
    }
}
