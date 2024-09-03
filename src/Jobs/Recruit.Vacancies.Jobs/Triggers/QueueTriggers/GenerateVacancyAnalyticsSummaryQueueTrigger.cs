using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateVacancyAnalyticsSummaryQueueTrigger
    {
        private readonly ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly AnalyticsEventStore _analyticsStore;
        private readonly IQueryStoreWriter _qsWriter;

        private string JobName => GetType().Name;

        public GenerateVacancyAnalyticsSummaryQueueTrigger(ILogger<GenerateVacancyAnalyticsSummaryQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig,
            AnalyticsEventStore analyticsStore, IQueryStoreWriter qsWriter)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _analyticsStore = analyticsStore;
            _qsWriter = qsWriter;
        }

        public async Task GenerateVacancyAnalyticsSummaryAsync([QueueTrigger(QueueNames.GenerateVacancyAnalyticsQueueName, Connection = "QueueStorage")]
            string message, TextWriter log)
        {
        
            _logger.LogDebug($"{JobName} is disabled, skipping ...");
        }
    }
}