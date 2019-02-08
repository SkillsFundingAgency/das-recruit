using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    internal abstract class TableStorageBase
    {
        private readonly TableStorageConnectionsDetails _config;
        private readonly ILogger<TableStorageBase> _logger;
        protected RetryPolicy RetryPolicy { get; }

        protected TableStorageBase(ILogger<TableStorageBase> logger, IOptions<TableStorageConnectionsDetails> config)
        {
            _config = config.Value;
            _logger = logger;
            RetryPolicy = StorageRetryPolicy.GetRetryPolicy(logger);
        }
    }
}
