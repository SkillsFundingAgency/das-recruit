using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateVacancyCommandHandler : IRequestHandler<UpdateVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;

        public UpdateVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging)
        {
            _repository = repository;
            _messaging = messaging;
        }

        public async Task Handle(UpdateVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancyUpdatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.Vacancy.EmployerAccountId
            });
        }
    }
}
