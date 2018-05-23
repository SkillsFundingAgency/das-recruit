using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo
{
    public class EditVacancyInfoUpdater
    {
        private readonly IJobsVacancyClient _client;
        private readonly ILogger<EditVacancyInfoUpdater> _logger;

        public EditVacancyInfoUpdater(IJobsVacancyClient client, ILogger<EditVacancyInfoUpdater> logger)
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