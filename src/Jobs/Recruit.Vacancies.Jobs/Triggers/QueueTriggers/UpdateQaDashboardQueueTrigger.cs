using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class UpdateQaDashboardQueueTrigger
    {
        private readonly ILogger<UpdateQaDashboardQueueTrigger> _logger;
        private readonly IQaDashboardProjectionService _projectionService;

        private string JobName => GetType().Name;

        public UpdateQaDashboardQueueTrigger(ILogger<UpdateQaDashboardQueueTrigger> logger, IQaDashboardProjectionService projectionService)
        {
            _logger = logger;
            _projectionService = projectionService;
        }

        public async Task UpdateQaDashboardAsync([QueueTrigger(QueueNames.UpdateQaDashboardQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            _logger.LogInformation("Starting rebuilding QA Dashboard");

            await _projectionService.RebuildQaDashboardAsync();

            _logger.LogInformation("Finished rebuilding QA Dashboard");
        }
    }
}
