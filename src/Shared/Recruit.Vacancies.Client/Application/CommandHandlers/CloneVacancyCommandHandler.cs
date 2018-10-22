using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloneVacancyCommandHandler: IRequestHandler<CloneVacancyCommand>
    {
        private readonly ILogger<CloneVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CloneVacancyCommandHandler(
            ILogger<CloneVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CloneVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cloning new vacancy with id: {vacancyId} from vacancy with id: {clonedVacancyId}", message.IdOfVacancyToClone, message.NewVacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.IdOfVacancyToClone);

            if (vacancy.Status != VacancyStatus.Submitted && vacancy.Status != VacancyStatus.Live && vacancy.Status != VacancyStatus.Closed)
            {
                _logger.LogError($"Unable to clone vacancy {{vacancyId}} due to it having a status of {vacancy.Status}.", message.IdOfVacancyToClone);
                
                throw new InvalidStateException($"Vacancy is not in correct state to be cloned. Current State: {vacancy.Status}");
            }

            SetClone(message, vacancy);

            await _repository.CreateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyCreatedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }

        private void SetClone(CloneVacancyCommand message, Vacancy vacancy)
        {
            var now = _timeProvider.Now;

            // Properties to replace
            vacancy.Id = message.NewVacancyId;
            vacancy.CreatedByUser = message.User;
            vacancy.CreatedDate = now;
            vacancy.LastUpdatedByUser = message.User;
            vacancy.LastUpdatedDate = now;
            vacancy.SourceOrigin = SourceOrigin.EmployerWeb;
            vacancy.SourceType = SourceType.Clone;
            vacancy.SourceVacancyReference = vacancy.VacancyReference;
            vacancy.Status = VacancyStatus.Draft;
            vacancy.IsDeleted = false;

            // Properties to remove
            vacancy.VacancyReference = null;
            vacancy.ApprovedDate = null;
            vacancy.ClosedDate = null;
            vacancy.ClosedByUser = null;
            vacancy.DeletedByUser = null;
            vacancy.DeletedDate = null;
            vacancy.LiveDate = null;
            vacancy.SubmittedByUser = null;
            vacancy.SubmittedDate = null;
        }
    }
}
