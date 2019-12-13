using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class PerformDataMigrationQueueTrigger
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly ILogger<PerformDataMigrationQueueTrigger> _logger;
        public PerformDataMigrationQueueTrigger(
            IVacancyRepository vacancyRepository, 
            IEmployerAccountProvider employerAccountProvider,
            ILogger<PerformDataMigrationQueueTrigger> logger)
        {
            _vacancyRepository = vacancyRepository;
            _employerAccountProvider = employerAccountProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            [QueueTrigger(QueueNames.DataMigrationQueueName, Connection = "QueueStorage")] DataMigrationQueueMessage message, 
            TextWriter log)
        {
            await PerformVacancyALEIdMigration(message);
        }

        private async Task PerformVacancyALEIdMigration(DataMigrationQueueMessage message)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);
            if (vacancy == null || vacancy.LegalEntityId == 0 || string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId) == false)
            {
                _logger.LogWarning($"Bypassing vacancy {message.VacancyId}");
                return;
            }
            var legalEntities = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(vacancy.EmployerAccountId);
            var selectedLegalEntity = legalEntities.FirstOrDefault(l => l.LegalEntityId == vacancy.LegalEntityId);
            if (selectedLegalEntity == null)
            {
                _logger.LogError($"Unable to find legal entity for vacancy {message.VacancyId}");
                return;
            }
            _logger.LogInformation($"Updating vacancy: {vacancy.Id} setting AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
            vacancy.AccountLegalEntityPublicHashedId = selectedLegalEntity.AccountLegalEntityPublicHashedId;
            await _vacancyRepository.UpdateAsync(vacancy);
            _logger.LogInformation($"Successfully updated vacancy: {vacancy.Id} with AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
        }
    }
}