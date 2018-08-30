using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer
{
    public class SetupEmployerUpdater
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<SetupEmployerUpdater> _logger;
        private readonly IEditVacancyInfoProjectionService _projectionService;

        public SetupEmployerUpdater(IJobsVacancyClient client, ILogger<SetupEmployerUpdater> logger, IEditVacancyInfoProjectionService projectionService)
        {
            _client = client;
            _logger = logger;
            _projectionService = projectionService;
        }

        internal async Task UpdateEditVacancyInfo(string employerAccountId)
        {
            var legalEntities = (await _client.GetEmployerLegalEntitiesAsync(employerAccountId)).ToList();

            await _projectionService.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);

            _logger.LogDebug("Legal Entities inserted: {count} for Employer: {EmployerAccountId}", legalEntities.Count, employerAccountId);
        }
    }
}