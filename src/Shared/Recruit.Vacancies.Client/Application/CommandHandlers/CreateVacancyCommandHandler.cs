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
    public class CreateVacancyCommandHandler: IRequestHandler<CreateVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CreateVacancyCommandHandler(IVacancyRepository repository, IMessaging messaging, ITimeProvider timeProvider)
        {
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CreateVacancyCommand message, CancellationToken cancellationToken)
        {
            var now = _timeProvider.Now;

            var vacancy = new Vacancy
            {
                Id = message.VacancyId,
                SourceOrigin = message.Origin,
                SourceType = SourceType.New,
                Title = message.Title,
                EmployerAccountId = message.EmployerAccountId,
                Status = VacancyStatus.Draft,
                CreatedDate = now,
                CreatedByUser = message.User,
                LastUpdatedDate = now,
                LastUpdatedByUser = message.User,
                IsDeleted = false
            };

            await _repository.CreateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyCreatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }
    }
}
