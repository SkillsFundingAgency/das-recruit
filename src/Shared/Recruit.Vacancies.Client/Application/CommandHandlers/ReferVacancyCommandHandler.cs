using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReferVacancyCommandHandler(
        ILogger<ReferVacancyCommandHandler> logger,
        IVacancyRepository repository) : IRequestHandler<ReferVacancyCommand, Unit>
    {
        public async Task<Unit> Handle(ReferVacancyCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Referring vacancy {vacancyReference}.", message.VacancyReference);

            var vacancy = await repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanRefer)
            {
                logger.LogWarning("Unable to refer vacancy {VacancyReference} due to vacancy having a status of {Status}.", vacancy.VacancyReference, vacancy.Status);
                return Unit.Value;
            }

            vacancy.Status = VacancyStatus.Referred;

            await repository.UpdateAsync(vacancy);
            
            return Unit.Value;
        }
    }
}