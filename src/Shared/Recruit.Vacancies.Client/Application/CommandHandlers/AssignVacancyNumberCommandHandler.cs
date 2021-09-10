using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class AssignVacancyNumberCommandHandler: IRequestHandler<AssignVacancyNumberCommand, Unit>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ILogger<AssignVacancyNumberCommandHandler> _logger;
        private readonly IGenerateVacancyNumbers _generator;

        public AssignVacancyNumberCommandHandler(
            IVacancyRepository repository,
            IMessaging messaging,
            ILogger<AssignVacancyNumberCommandHandler> logger, 
            IGenerateVacancyNumbers generator)
        {
            _repository = repository;
            _messaging = messaging;
            _logger = logger;
            _generator = generator;
        }

        public async Task<Unit> Handle(AssignVacancyNumberCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);
            
            if (vacancy.VacancyReference.HasValue)
            {
                _logger.LogInformation("Vacancy: {vacancyId} already has a vacancy number: {vacancyNumber}. Will not be changed.", vacancy.Id, vacancy.VacancyReference);
                return Unit.Value;
            }

            _logger.LogInformation("Assigning vacancy number for vacancy {vacancyId}.", message.VacancyId);

            vacancy.VacancyReference = await _generator.GenerateAsync();

            await _repository.UpdateAsync(vacancy);
            await _messaging.PublishEvent(new DraftVacancyUpdatedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });

            _logger.LogInformation("Updated Vacancy: {vacancyId} with vacancy number: {vacancyNumber}", vacancy.Id, vacancy.VacancyReference);
            return Unit.Value;
        }
    }
}
