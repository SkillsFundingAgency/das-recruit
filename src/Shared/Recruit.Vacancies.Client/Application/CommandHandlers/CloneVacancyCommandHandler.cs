using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloneVacancyCommandHandler(
        ILogger<CloneVacancyCommandHandler> logger,
        IVacancyRepository repository,
        ITimeProvider timeProvider)
        : IRequestHandler<CloneVacancyCommand, Unit>
    {
        public async Task<Unit> Handle(CloneVacancyCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Cloning new vacancy with id: {vacancyId} from vacancy with id: {clonedVacancyId}", message.IdOfVacancyToClone, message.NewVacancyId);

            var vacancy = await repository.GetVacancyAsync(message.IdOfVacancyToClone);

            if (vacancy.Status != VacancyStatus.Submitted && vacancy.Status != VacancyStatus.Live && vacancy.Status != VacancyStatus.Closed && vacancy.Status != VacancyStatus.Review)
            {
                logger.LogError($"Unable to clone vacancy {{vacancyId}} due to it having a status of {vacancy.Status}.", message.IdOfVacancyToClone);
                
                throw new InvalidStateException($"Vacancy is not in correct state to be cloned. Current State: {vacancy.Status}");
            }

            var clone = CreateClone(message, vacancy);

            await repository.CreateAsync(clone);
           
            return Unit.Value;
        }

        private Vacancy CreateClone(CloneVacancyCommand message, Vacancy vacancy)
        {
            var now = timeProvider.Now;
            var clone = JsonConvert.DeserializeObject<Vacancy>(JsonConvert.SerializeObject(vacancy));

            // Properties to replace
            clone.Id = message.NewVacancyId;
            clone.CreatedByUser = message.User;
            clone.CreatedDate = now;
            clone.LastUpdatedDate = now;
            clone.SourceOrigin = message.SourceOrigin;
            clone.SourceType = SourceType.Clone;
            clone.SourceVacancyReference = vacancy.VacancyReference;
            clone.Status = VacancyStatus.Draft;
            clone.IsDeleted = false;
            clone.ClosingDate = message.ClosingDate;
            clone.StartDate = message.StartDate;
            clone.HasOptedToAddQualifications = vacancy.HasOptedToAddQualifications ?? vacancy.Qualifications is { Count: > 0 };

            // Properties to remove
            clone.VacancyReference = null;
            clone.ApprovedDate = null;
            clone.ClosedDate = null;
            clone.DeletedDate = null;
            clone.LiveDate = null;
            clone.SubmittedByUser = null;
            clone.SubmittedDate = null;
            clone.ClosureReason = null;
            clone.TransferInfo = null;
            clone.ReviewByUser = null;
            clone.ReviewDate = null;
            clone.ReviewCount = 0;
            clone.EmployerReviewFieldIndicators = null;
            clone.EmployerRejectedReason = null;
            clone.ProviderReviewFieldIndicators = null;
            
            MigrateLocations(clone);

            return clone;
        }
        
        private static void MigrateLocations(Vacancy clone)
        {
            switch (clone)
            {
                case { EmployerLocation: not null }:
                    clone.EmployerLocationOption = AvailableWhere.OneLocation;
                    clone.EmployerLocations = [clone.EmployerLocation];
                    clone.EmployerLocation = null;
                    return;
                case { EmployerLocationInformation: not null, EmployerLocationOption: null }:
                    clone.EmployerLocationOption = AvailableWhere.AcrossEngland;
                    return;
                case { EmployerLocations.Count: > 0, EmployerLocationOption: null }:
                    clone.EmployerLocationOption = clone.EmployerLocations.Count == 1
                        ? AvailableWhere.OneLocation
                        : AvailableWhere.MultipleLocations;
                    break;
            }
        }
    }
}
