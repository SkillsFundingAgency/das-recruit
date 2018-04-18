using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo
{
    public class EditVacancyInfoUpdater
    {
        private readonly IJobsVacancyClient _client;

        public EditVacancyInfoUpdater(IJobsVacancyClient client)
        {
            _client = client;
        }

        internal async Task UpdateEditVacancyInfo(string employerAccountId)
        {
            var legalEntities = await _client.GetEmployerLegalEntitiesAsync(employerAccountId);

            await _client.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
        }
    }
}