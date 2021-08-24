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

            if (vacancy.Status != VacancyStatus.Submitted && vacancy.Status != VacancyStatus.Live && vacancy.Status != VacancyStatus.Closed && vacancy.Status != VacancyStatus.Review)
            {
                _logger.LogError($"Unable to clone vacancy {{vacancyId}} due to it having a status of {vacancy.Status}.", message.IdOfVacancyToClone);
                
                throw new InvalidStateException($"Vacancy is not in correct state to be cloned. Current State: {vacancy.Status}");
            }

            var clone = CreateClone(message, vacancy);

            await _repository.CreateAsync(clone);

            await _messaging.PublishEvent(new VacancyCreatedEvent
            {
                VacancyId = clone.Id
            });
        }

        private Vacancy CreateClone(CloneVacancyCommand message, Vacancy vacancy)
        {
            var now = _timeProvider.Now;

            var clone = JsonConvert.DeserializeObject<Vacancy>(JsonConvert.SerializeObject(vacancy));

            // Properties to replace
            clone.Id = message.NewVacancyId;
            clone.CreatedByUser = message.User;
            clone.CreatedDate = now;
            clone.LastUpdatedByUser = message.User;
            clone.LastUpdatedDate = now;
            clone.SourceOrigin = message.SourceOrigin;
            clone.SourceType = SourceType.Clone;
            clone.SourceVacancyReference = vacancy.VacancyReference;
            clone.Status = VacancyStatus.Draft;
            clone.IsDeleted = false;
            clone.ClosingDate = message.ClosingDate;
            clone.StartDate = message.StartDate;

            // Properties to remove
            clone.VacancyReference = null;
            clone.ApprovedDate = null;
            clone.ClosedDate = null;
            clone.ClosedByUser = null;
            clone.DeletedByUser = null;
            clone.DeletedDate = null;
            clone.LiveDate = null;
            clone.SubmittedByUser = null;
            clone.SubmittedDate = null;
            clone.ClosureReason = null;
            clone.ClosureExplanation = null;
            clone.TransferInfo = null;
            clone.ReviewByUser = null;
            clone.ReviewDate = null;
            clone.ReviewCount = 0;

            return clone;
        }
    }
}
