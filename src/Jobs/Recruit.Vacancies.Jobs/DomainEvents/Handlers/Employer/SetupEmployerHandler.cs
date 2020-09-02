using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer
{
    public class SetupEmployerHandler : DomainEventHandler,  IDomainEventHandler<SetupEmployerEvent>
    {
        private readonly ILogger<SetupEmployerHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IEditVacancyInfoProjectionService _projectionService;

        public SetupEmployerHandler(ILogger<SetupEmployerHandler> logger, IJobsVacancyClient client, IEditVacancyInfoProjectionService projectionService) : base(logger)
        {
            _logger = logger;
            _client = client;
            _projectionService = projectionService;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<SetupEmployerEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(SetupEmployerEvent)} for Account: {{AccountId}}", @event.EmployerAccountId);

                var legalEntities = (await _client.GetEmployerLegalEntitiesAsync(@event.EmployerAccountId)).ToList();

                var vacancyDataTask =  _projectionService.UpdateEmployerVacancyDataAsync(@event.EmployerAccountId, legalEntities);

                var employerProfilesTask = _client.RefreshEmployerProfiles(@event.EmployerAccountId, legalEntities.Select(x => x.AccountLegalEntityPublicHashedId));

                await Task.WhenAll(vacancyDataTask, employerProfilesTask);

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

