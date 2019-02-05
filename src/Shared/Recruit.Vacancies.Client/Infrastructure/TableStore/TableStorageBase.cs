using System;
using System.Linq;
using System.Security.Authentication;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    internal abstract class TableStorageBase
    {
        private readonly TableStorageConnectionsDetails _config;
        private readonly Lazy<ILogger> _tblStorageCommandLogger;
        //private readonly string[] _excludedCommands = { "isMaster", "buildInfo", "saslStart", "saslContinue", "getLastError" };

        protected ILogger Logger { get; }

        //protected RetryPolicy RetryPolicy { get; }

        protected TableStorageBase(ILoggerFactory loggerFactory, IOptions<TableStorageConnectionsDetails> config)
        {
            _config = config.Value;

            Logger = loggerFactory.CreateLogger(this.GetType().FullName);
            _tblStorageCommandLogger = new Lazy<ILogger>(() => loggerFactory.CreateLogger("CloudTable Storage command"));

            //RetryPolicy = MongoDbRetryPolicy.GetRetryPolicy(Logger);
        }
    }
}
