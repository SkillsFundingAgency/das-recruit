using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class AssignVacancyNumberCommandHandler(
    IVacancyRepository repository,
    ILogger<AssignVacancyNumberCommandHandler> logger,
    IJobsVacancyClient jobsVacancyClient)
    : IRequestHandler<AssignVacancyNumberCommand, Unit>
{
    public async Task<Unit> Handle(AssignVacancyNumberCommand message, CancellationToken cancellationToken)
    {
        var vacancy = await repository.GetVacancyAsync(message.VacancyId);
        if (vacancy.VacancyReference.HasValue)
        {
            logger.LogInformation("Vacancy '{VacancyId}' already has a vacancy reference ({VacancyReference}).", vacancy.Id, vacancy.VacancyReference);
            return Unit.Value;
        }
        
        logger.LogInformation("Assigning reference '{VacancyReference}' to vacancy '{VacancyId}'.", vacancy.VacancyReference, message.VacancyId);
        await repository.UpdateAsync(vacancy);
        await jobsVacancyClient.PatchTrainingProviderAsync(vacancy.Id);
        return Unit.Value;
    }
}