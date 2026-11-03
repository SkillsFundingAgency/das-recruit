using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class RejectVacancyCommandHandler(
    ILogger<RejectVacancyCommandHandler> logger,
    IVacancyRepository repository,
    IMessaging messaging,
    IOuterApiClient outerApiClient)
    : IRequestHandler<RejectVacancyCommand, Unit>
{
    private readonly IMessaging _messaging = messaging;

    public async Task<Unit> Handle(RejectVacancyCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Rejecting vacancy {vacancyReference}.", message.VacancyReference);

        var vacancy = await repository.GetVacancyAsync(message.VacancyReference);

        if (!vacancy.CanReview)
        {
            logger.LogWarning($"Unable to refer vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
            return Unit.Value;
        }

        vacancy.Status = VacancyStatus.Rejected;

        await repository.UpdateAsync(vacancy);

        await outerApiClient.Post(new PostEmployerRejectedVacancyEventRequest(new PostEmployerRejectedVacancyEventData(vacancy.Id)));
            
        return Unit.Value;
    }
}