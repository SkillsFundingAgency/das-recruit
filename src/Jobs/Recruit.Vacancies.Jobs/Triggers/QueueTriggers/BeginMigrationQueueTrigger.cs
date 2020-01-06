using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class BeginMigrationQueueTrigger
    {
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IRecruitQueueService _recruitQueueService;
        private readonly ILogger<BeginMigrationQueueTrigger> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        public BeginMigrationQueueTrigger(
            IVacancyRepository vacancyRepository, 
            IVacancyQuery vacancyQuery, 
            IRecruitQueueService recruitQueueService,
            IEmployerAccountProvider employerAccountProvider,
            ILogger<BeginMigrationQueueTrigger> logger)
        {
            _vacancyQuery = vacancyQuery;
            _recruitQueueService = recruitQueueService;
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _employerAccountProvider = employerAccountProvider;
        }

        public async Task BeginMigrationAsync(
            [QueueTrigger(QueueNames.BeginMigrationQueueName, Connection = "QueueStorage")] CloudQueueMessage message, 
            TextWriter log)
        {
            if (message.DequeueCount == 1)
            {
                try 
                {
                    await RunVacancyALEIdMigrationAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred when trying to queue up vacancies for ALE Id update");
                }
            }
            else
            {
                _logger.LogWarning("This message has reappeared. Message will be disposed.");
            }
        }

        private async Task RunVacancyALEIdMigrationAsync()
        {
            _logger.LogInformation("Begining queuing vacancies for ALE Id migration process");
            var tasks = new List<Task>();
            var vacancyIds = await _vacancyQuery.GetAllVacancyIdsAsync();
            _logger.LogInformation($"Found {vacancyIds.Count()} vacancies for ALE Id migration");

            var maxDegreeOfParallelism = Environment.ProcessorCount * 10;

            Parallel.ForEach(
                vacancyIds, 
                new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, 
                (vacancyId) => PerformVacancyALEIdMigration(vacancyId).Wait());
        }

        private async Task PerformVacancyALEIdMigration(Guid vacancyId)
        {
            _logger.LogInformation($"Carrying out ALEId migration on vacancy {vacancyId} ");
            Vacancy vacancy;
            try
            {
                vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching vacancy - Bypassing vacancy {vacancyId}");
                return;
            }

            if (vacancy == null)
            {
                _logger.LogWarning($"Not found - Bypassing vacancy {vacancyId}");
                return;
            }

            if (vacancy.LegalEntityId == 0) 
            {
                _logger.LogWarning($"Missing legalEntity - Bypassing vacancy {vacancyId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId) == false)
            {
                _logger.LogWarning($"Already updated - Bypassing vacancy {vacancyId}");
                return;
            }

            LegalEntityViewModel selectedLegalEntity;
            try
            {
                var legalEntities = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(vacancy.EmployerAccountId);
                selectedLegalEntity = legalEntities.FirstOrDefault(l => l.LegalEntityId == vacancy.LegalEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching legal entity details for vacancy {vacancyId}");
                return;
            }

            if (selectedLegalEntity == null)
            {
                _logger.LogError($"Unable to find legal entity for vacancy {vacancyId}");
                return;
            }

            _logger.LogInformation($"Updating vacancy: {vacancyId} setting AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
            vacancy.AccountLegalEntityPublicHashedId = selectedLegalEntity.AccountLegalEntityPublicHashedId;
            await _vacancyRepository.UpdateAsync(vacancy);
            _logger.LogInformation($"Successfully updated vacancy: {vacancyId} with AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
        }
    }
}