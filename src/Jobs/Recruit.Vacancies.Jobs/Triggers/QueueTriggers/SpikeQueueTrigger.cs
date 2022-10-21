using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;

#if DEBUG
namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class SpikeQueueTrigger
    {
        private readonly ILogger<SpikeQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IApplicationReviewQuery _query;
        private readonly EncodingConfig _encodingConfig;
        private readonly IEncodingService _encodingService;

        private string JobName => GetType().Name;

        public SpikeQueueTrigger(ILogger<SpikeQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IApplicationReviewQuery query, EncodingConfig encodingConfig, IEncodingService encodingService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _query = query;
            _encodingConfig = encodingConfig;
            _encodingService = encodingService;
        }

        public async Task SpikeAsync([QueueTrigger("test-queue", Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            var employerAccountId = Environment.GetEnvironmentVariable("EmployerAccount");
            //var result = await _query.GetEmployerOwnedVacancySummariesByEmployerAccountAsync(employerAccountId);
            var result = await _query.GetAllVacancyReferencesAsync();
        }
    }
}
#endif