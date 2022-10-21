using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    public class QueryStoreTableChecker
    {
        private readonly ILogger<QueryStoreTableChecker> _logger;
        private CloudTable CloudTable { get; }

        public QueryStoreTableChecker(ILogger<QueryStoreTableChecker> logger, IOptions<TableStorageConnectionsDetails> config)
        {
            _logger = logger;
            var storageAccount = CloudStorageAccount.Parse(config.Value.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            CloudTable = tableClient.GetTableReference(StorageTableNames.QueryStore);
        }

        public void EnsureQueryStoreTableExist()
        {
            _logger.LogInformation("Ensuring Query Store table Exists.");

            CloudTable.CreateIfNotExistsAsync();
        }
    }
}
