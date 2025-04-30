using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class DeleteStaleQueryStoreDocumentsQueueTrigger
    {
        private readonly ILogger<DeleteStaleQueryStoreDocumentsQueueTrigger> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IQueryStoreHouseKeepingService _queryStoreHouseKeepingService;
        private string JobName => GetType().Name;

        private const int DefaultStaleByDays = 90;

        public DeleteStaleQueryStoreDocumentsQueueTrigger(ILogger<DeleteStaleQueryStoreDocumentsQueueTrigger> logger, 
            ITimeProvider timeProvider,
            IQueryStoreHouseKeepingService queryStoreHouseKeepingService)
        {
            _logger = logger;
            _timeProvider = timeProvider;
            _queryStoreHouseKeepingService = queryStoreHouseKeepingService;
        }

        public async Task DeleteStaleQueryStoreDocumentsAsync([QueueTrigger(QueueNames.DeleteStaleQueryStoreDocumentsQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {

                var payload = JsonConvert.DeserializeObject<DeleteStaleQueryStoreDocumentsQueueMessage>(message);

                var targetDate = payload?.CreatedByScheduleDate ?? _timeProvider.Today;

                var documentsStaleByDate = targetDate.AddDays((DefaultStaleByDays) * -1);
                
                _logger.LogInformation($"Begining to delete query store stale documents that have not been updated since {documentsStaleByDate.ToShortDateString()}");

                var documentTypesToDelete = new [] 
                {
                    "EditVacancyInfo", 
                    nameof(EmployerDashboard),
                    nameof(ProviderDashboard),
                    nameof(ClosedVacancy)
                };

                foreach (var viewType in documentTypesToDelete)
                {
                    var staleDocuments = await _queryStoreHouseKeepingService.GetStaleDocumentsAsync<QueryProjectionBase>(viewType, documentsStaleByDate);
                    _logger.LogInformation($"Found {staleDocuments.Count} query store documents for type {viewType} updated before {documentsStaleByDate}");
                    if (staleDocuments.Any())
                    {
                        var deletedCount = await _queryStoreHouseKeepingService.DeleteStaleDocumentsAsync<QueryProjectionBase>(viewType, staleDocuments.Select(s => s.Id));
                        _logger.LogInformation($"Deleted {deletedCount} query store documents for type {viewType} updated before {documentsStaleByDate}");
                    }
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