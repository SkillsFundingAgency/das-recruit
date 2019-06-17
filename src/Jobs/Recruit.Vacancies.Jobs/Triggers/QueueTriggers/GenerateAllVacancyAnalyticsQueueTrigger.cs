using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateAllVacancyAnalyticsQueueTrigger
    {
        private readonly ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IRecruitQueueService _queue;
        private readonly IVacancyQuery _vacancyQuery;

        private string JobName => GetType().Name;

        public GenerateAllVacancyAnalyticsQueueTrigger(ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig,
            IRecruitQueueService queue, IVacancyQuery vacancyQuery)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _queue = queue;
            _vacancyQuery = vacancyQuery;
        }

        public async Task GenerateAllVacancyAnalyticsAsync([QueueTrigger(QueueNames.GenerateAllVacancyAnalyticsSummariesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            try
            {
                _logger.LogInformation($"Starting {this.GetType().Name}");

                var allVacancyReferences = await _vacancyQuery.GetAllVacancyReferencesAsync();

                _logger.LogInformation($"Adding analytics generation messages for {allVacancyReferences.Count()} vacancies");

                foreach (var vacancyReference in allVacancyReferences)
                {
                    var queueMessage = new VacancyAnalyticsQueueMessage
                    {
                        VacancyReference = vacancyReference
                    };

                    await _queue.AddMessageAsync(queueMessage);
                }

                _logger.LogInformation($"Finished {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to add analytics generation messages");
                throw;
            }
        }
    }
}