using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateEmployerOwnedVacancyCommandHandler: IRequestHandler<CreateEmployerOwnedVacancyCommand>
    {
        private readonly ILogger<CreateEmployerOwnedVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CreateEmployerOwnedVacancyCommandHandler(
            ILogger<CreateEmployerOwnedVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CreateEmployerOwnedVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating vacancy with id {vacancyId}.", message.VacancyId);

            var now = _timeProvider.Now;

            var vacancy = new Vacancy
            {
                Id = message.VacancyId,
                OwnerType = message.UserType == UserType.Provider ? OwnerType.Provider : OwnerType.Employer,
                SourceOrigin = message.Origin,
                SourceType = SourceType.New,
                Title = message.Title,
                EmployerAccountId = message.EmployerAccountId,
                Status = VacancyStatus.Draft,
                CreatedDate = now,
                CreatedByUser = message.User,
                LastUpdatedDate = now,
                LastUpdatedByUser = message.User,
                IsDeleted = false,
                TrainingProvider = message.TrainingProvider,
                ProgrammeId = message.ProgrammeId
            };

            await _repository.CreateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyCreatedEvent
            {
                VacancyId = vacancy.Id
            });
        }
    }
}
