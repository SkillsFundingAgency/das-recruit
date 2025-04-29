using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateAllBlockedOrganisationsQueryStoreDocumentsQueueTrigger
    {
        private readonly ILogger<GenerateAllBlockedOrganisationsQueryStoreDocumentsQueueTrigger> _logger;
        private readonly IBlockedOrganisationsProjectionService _projectionService;
        private string JobName => GetType().Name;

        public GenerateAllBlockedOrganisationsQueryStoreDocumentsQueueTrigger(ILogger<GenerateAllBlockedOrganisationsQueryStoreDocumentsQueueTrigger> logger, IBlockedOrganisationsProjectionService projectionService)
        {
            _logger = logger;
            _projectionService = projectionService;
        }

        public async Task GenerateAllEmployerDashboardsAsync([QueueTrigger(QueueNames.GenerateAllBlockedOrganisationsQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                _logger.LogInformation($"Start {JobName}");

                await _projectionService.RebuildAllBlockedOrganisationsAsync();

                _logger.LogInformation($"Finished {JobName}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialise event: {eventBody}", message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
                throw;
            }
        }
    }
}