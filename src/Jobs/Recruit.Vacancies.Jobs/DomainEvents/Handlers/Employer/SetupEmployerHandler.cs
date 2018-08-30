using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class SetupEmployerHandler : DomainEventHandler,  IDomainEventHandler<SetupEmployerEvent>
    {
        private readonly ILogger<SetupEmployerHandler> _logger;
        private readonly SetupEmployerUpdater _updater;

        public SetupEmployerHandler(ILogger<SetupEmployerHandler> logger, SetupEmployerUpdater updater) : base(logger)
        {
            _logger = logger;
            _updater = updater;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<SetupEmployerEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);

                await _updater.UpdateEditVacancyInfo(@event.EmployerAccountId);

                _logger.LogInformation($"Finished Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

