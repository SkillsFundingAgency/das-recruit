using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateProviderOwnedVacancyCommandHandler: IRequestHandler<CreateProviderOwnedVacancyCommand, Unit>
    {
        private readonly ILogger<CreateProviderOwnedVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ServiceParameters _serviceParameters;

        public CreateProviderOwnedVacancyCommandHandler(
            ILogger<CreateProviderOwnedVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider,
            ServiceParameters serviceParameters)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _serviceParameters = serviceParameters;
        }

        public async Task<Unit> Handle(CreateProviderOwnedVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating vacancy with id {vacancyId}.", message.VacancyId);

            var now = _timeProvider.Now;

            var vacancy = new Vacancy
            {
                Id = message.VacancyId,
                OwnerType = message.UserType == UserType.Provider ? OwnerType.Provider : OwnerType.Employer,
                SourceOrigin = message.Origin,
                SourceType = SourceType.New,
                EmployerAccountId = message.EmployerAccountId,
                AccountLegalEntityPublicHashedId = message.AccountLegalEntityPublicHashedId,
                LegalEntityName = message.LegalEntityName,
                TrainingProvider = new TrainingProvider { Ukprn = message.Ukprn },
                Status = VacancyStatus.Draft,
                CreatedDate = now,
                CreatedByUser = message.User,
                LastUpdatedDate = now,
                LastUpdatedByUser = message.User,
                IsDeleted = false,
                Title = message.Title,
                VacancyType = _serviceParameters.VacancyType,
                ApplicationMethod = _serviceParameters.VacancyType.GetValueOrDefault() == VacancyType.Traineeship 
                    ? ApplicationMethod.ThroughFindATraineeship : (ApplicationMethod?)null
            };

            await _repository.CreateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyCreatedEvent
            {
                VacancyId = vacancy.Id
            });
            return Unit.Value;
        }
    }
}
