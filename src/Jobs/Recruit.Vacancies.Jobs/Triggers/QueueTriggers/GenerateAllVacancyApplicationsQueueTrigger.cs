using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateAllVacancyApplicationsQueueTrigger
    {
        private readonly IVacancyApplicationsProjectionService _projectionService;
        private readonly ILogger<GenerateAllVacancyApplicationsQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IApplicationReviewQuery _query;

        private string JobName => GetType().Name;

        public GenerateAllVacancyApplicationsQueueTrigger(ILogger<GenerateAllVacancyApplicationsQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, IVacancyApplicationsProjectionService projectionService, IApplicationReviewQuery query)
        {
            _logger = logger;
            _projectionService = projectionService;
            _jobsConfig = jobsConfig;
            _query = query;
        }

        public async Task GenerateAllVacancyApplicationsAsync([QueueTrigger(QueueNames.GenerateAllVacancyApplicationsQueueName, Connection = "QueueStorage")]
            string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(JobName))
            {
                _logger.LogDebug($"{JobName} is disabled, skipping ...");
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    var allVacancyReferences = await _query.GetAllVacancyReferencesAsync();

                    foreach (var vacancyReference in allVacancyReferences)
                    {
                        await _projectionService.UpdateVacancyApplicationsAsync(vacancyReference);
                    }
                    
                    _logger.LogInformation($"Finished {JobName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
                throw;
            }
        }
    }
}
