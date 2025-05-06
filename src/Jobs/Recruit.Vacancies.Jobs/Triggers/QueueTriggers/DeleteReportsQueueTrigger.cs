using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class DeleteReportsQueueTrigger
    {
        private const int DeleteReportAfterTimeSpanDays = 7;

        private readonly ILogger<DeleteReportsQueueTrigger> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IReportRepository _reportRepository;

        private string JobName => GetType().Name;

        public DeleteReportsQueueTrigger(ILogger<DeleteReportsQueueTrigger> logger, 
            ITimeProvider timeProvider,
            IReportRepository reportRepository)
        {
            _logger = logger;
            _timeProvider = timeProvider;
            _reportRepository = reportRepository;
        }

        public async Task DeleteReportsAsync([QueueTrigger(QueueNames.DeleteReportsQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                var deleteReportsCreatedBeforeDate = _timeProvider.Today.AddDays(DeleteReportAfterTimeSpanDays * -1);

                var deletedCount = await _reportRepository.DeleteReportsCreatedBeforeAsync(deleteReportsCreatedBeforeDate);

                _logger.LogInformation($"Deleted {deletedCount} reports created before {deleteReportsCreatedBeforeDate}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {JobName}");
                throw;
            }
        }
    }
}
