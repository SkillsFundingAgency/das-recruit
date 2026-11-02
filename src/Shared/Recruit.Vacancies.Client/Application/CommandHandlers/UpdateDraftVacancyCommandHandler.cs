using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class UpdateDraftVacancyCommandHandler(
    ILogger<UpdateDraftVacancyCommandHandler> logger,
    IVacancyRepository repository,
    ITimeProvider timeProvider, 
    IJobsVacancyClient jobsVacancyClient)
    : IRequestHandler<UpdateDraftVacancyCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDraftVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating vacancy {vacancyId}.", message.Vacancy.Id);

        message.Vacancy.LastUpdatedDate = timeProvider.Now;

        await repository.UpdateAsync(message.Vacancy);

        await jobsVacancyClient.PatchTrainingProviderAsync(message.Vacancy.Id);
            
        return Unit.Value;
    }
}