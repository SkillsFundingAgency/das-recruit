using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateReportQueueTrigger
    {
        private readonly ILogger<GenerateReportQueueTrigger> _logger;
        private readonly IReportService _reportService;

        private string JobName => GetType().Name;

        public GenerateReportQueueTrigger(ILogger<GenerateReportQueueTrigger> logger, 
            IReportService reportService)
        {
            _logger = logger;
            _reportService = reportService;
        }

        public async Task GenerateReportAsync([QueueTrigger(QueueNames.ReportQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    var reportMessage = JsonConvert.DeserializeObject<ReportQueueMessage>(message);

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
