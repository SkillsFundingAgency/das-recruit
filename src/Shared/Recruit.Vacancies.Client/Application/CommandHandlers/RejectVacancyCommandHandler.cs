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

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class RejectVacancyCommandHandler : IRequestHandler<RejectVacancyCommand, Unit>
    {
        private readonly ILogger<RejectVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly IOuterApiClient _outerApiClient;

        public RejectVacancyCommandHandler(
            ILogger<RejectVacancyCommandHandler> logger,
            IVacancyRepository repository,
            IMessaging messaging,
            IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _outerApiClient = outerApiClient;
        }

        public async Task<Unit> Handle(RejectVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rejecting vacancy {vacancyReference}.", message.VacancyReference);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyReference);

            if (!vacancy.CanReview)
            {
                _logger.LogWarning($"Unable to refer vacancy {{vacancyReference}} due to vacancy having a status of {vacancy.Status}.", vacancy.VacancyReference);
                return Unit.Value;
            }

            vacancy.Status = VacancyStatus.Rejected;

            await _repository.UpdateAsync(vacancy);

            await _outerApiClient.Post(new PostEmployerRejectedVacancyEventRequest(new PostEmployerRejectedVacancyEventData(vacancy.Id)));
            
            return Unit.Value;
        }
    }
}
