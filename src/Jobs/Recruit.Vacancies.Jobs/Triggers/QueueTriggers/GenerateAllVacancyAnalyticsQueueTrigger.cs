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

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;

public class GenerateAllVacancyAnalyticsQueueTrigger(
    ILogger<GenerateAllVacancyAnalyticsQueueTrigger> logger,
    RecruitWebJobsSystemConfiguration jobsConfig,
    IRecruitQueueService queue,
    IVacancyQuery vacancyQuery)
{
    private const string JobName = nameof(GenerateAllVacancyAnalyticsQueueTrigger);

    public async Task GenerateAllVacancyAnalyticsAsync([QueueTrigger(QueueNames.GenerateAllVacancyAnalyticsSummariesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
    {
        if (jobsConfig.DisabledJobs.Contains(JobName))
        {
            logger.LogDebug("{JobName} is disabled, skipping ...", JobName);
            return;
        }
            
        try
        {
            logger.LogInformation("Starting {JobName}", JobName);
            var allVacancyReferences = await vacancyQuery.GetAllVacancyReferencesAsync();

            logger.LogInformation("Adding analytics generation messages for {allVacancyReferencesCount} vacancies", allVacancyReferences.Count());
            foreach (long vacancyReference in allVacancyReferences)
            {
                var queueMessage = new VacancyAnalyticsQueueMessage { VacancyReference = vacancyReference };
                await queue.AddMessageAsync(queueMessage);
            }

            logger.LogInformation("Finished {JobName}", JobName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to add analytics generation messages");
            throw;
        }
    }
}