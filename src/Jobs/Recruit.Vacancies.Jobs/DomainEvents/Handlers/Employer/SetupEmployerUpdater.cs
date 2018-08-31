using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer
{
    public class SetupEmployerUpdater
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<SetupEmployerUpdater> _logger;

        public SetupEmployerUpdater(IJobsVacancyClient client, ILogger<SetupEmployerUpdater> logger)
        {
            _client = client;
            _logger = logger;
        }

        internal async Task UpdateEditVacancyInfo(string employerAccountId)
        {
            var legalEntities = (await _client.GetEmployerLegalEntitiesAsync(employerAccountId)).ToList();

            await _client.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);

            _logger.LogDebug("Legal Entities inserted: {count} for Employer: {EmployerAccountId}", legalEntities.Count, employerAccountId);
        }
    }
}