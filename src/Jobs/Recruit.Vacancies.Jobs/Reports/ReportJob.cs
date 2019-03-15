using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Reports
{
    public class ReportJob
    {
        private readonly ILogger<ReportJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IReportService _reportService;

        private string JobName => GetType().Name;

        public ReportJob(ILogger<ReportJob> logger, 
            RecruitWebJobsSystemConfiguration jobsConfig,
            IReportService reportService)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _reportService = reportService;
        }

        public async Task GenerateReport([QueueTrigger(QueueNames.ReportQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                if (_jobsConfig.DisabledJobs.Contains(JobName))
                {
                    _logger.LogDebug($"{JobName} is disabled, skipping ...");
                    return;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    var reportMessage = JsonConvert.DeserializeObject<StorageQueueReportMessage>(message);

                    await _reportService.GenerateReportAsync(reportMessage.ReportId);

                    _logger.LogInformation($"Finished {JobName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {JobName}");
                throw;
            }
        }
    }
}
