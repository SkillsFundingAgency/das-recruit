using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;

        public DeleteVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging)
        {
            _repository = repository;
            _messaging = messaging;
        }

        public async Task Handle(DeleteVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancyDeletedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.Vacancy.EmployerAccountId,
                VacancyId = message.Vacancy.Id
            });
        }
    }
}
