using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class CreateProviderOwnedVacancyCommandHandler(
    ILogger<CreateProviderOwnedVacancyCommandHandler> logger,
    IVacancyRepository repository,
    ITimeProvider timeProvider)
    : IRequestHandler<CreateProviderOwnedVacancyCommand, Unit>
{
    public async Task<Unit> Handle(CreateProviderOwnedVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating vacancy with id {vacancyId}.", message.VacancyId);

        var now = timeProvider.Now;

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
            IsDeleted = false,
            Title = message.Title,
            ApplicationMethod = null
        };

        await repository.CreateAsync(vacancy);
       
        return Unit.Value;
    }
}