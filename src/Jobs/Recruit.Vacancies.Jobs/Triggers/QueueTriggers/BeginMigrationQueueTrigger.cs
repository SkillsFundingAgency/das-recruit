using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class BeginMigrationQueueTrigger
    {
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IRecruitQueueService _recruitQueueService;
        private readonly ILogger<BeginMigrationQueueTrigger> _logger;
        public BeginMigrationQueueTrigger(
            IVacancyQuery vacancyQuery, 
            IRecruitQueueService recruitQueueService,
            ILogger<BeginMigrationQueueTrigger> logger)
        {
            _vacancyQuery = vacancyQuery;
            _recruitQueueService = recruitQueueService;
            _logger = logger;
        }

        public async Task BeginMigrationAsync(
            [QueueTrigger(QueueNames.BeginMigrationQueueName, Connection = "QueueStorage")] string message, 
            TextWriter log)
        {
            await RunVacancyALEIdMigrationAsync();
        }

        private async Task RunVacancyALEIdMigrationAsync()
        {
            _logger.LogInformation("Begining queuing vacancies for ALE Id migration process");
            var tasks = new List<Task>();
            var vacancyIds = await _vacancyQuery.GetAllVacancyIdsAsync();
            _logger.LogInformation($"Found {vacancyIds.Count()} vacancies for ALE Id migration");
            foreach (var vacancyId in vacancyIds)
                tasks.Add(SendVacancyALEIdMigrationMessage(vacancyId));
            await Task.WhenAll(tasks);
        }

        private Task SendVacancyALEIdMigrationMessage(Guid vacancyId)
        {
            _logger.LogInformation($"Queueing up vacancy {vacancyId} for ALE Id migration");
            var message = new DataMigrationQueueMessage {VacancyId = vacancyId};
            return _recruitQueueService.AddMessageAsync<DataMigrationQueueMessage>(message);
        }
    }

}