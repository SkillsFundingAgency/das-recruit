using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo
{
    public class EditVacancyInfoUpdater
    {
        private readonly ILogger<EditVacancyInfoUpdater> _logger;
        private readonly IJobsVacancyClient _client;

        public EditVacancyInfoUpdater(ILogger<EditVacancyInfoUpdater> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        internal async Task UpdateEditVacancyInfo(string employerAccountId)
        {
            var legalEntities = await _client.GetEmployerLegalEntitiesAsync(employerAccountId);

            await _client.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
        }
    }
}