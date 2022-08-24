using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using SFA.DAS.Recruit.Api.Configuration;

namespace SFA.DAS.Recruit.Api.Services
{
    public class QueryStoreClient : IQueryStoreReader
    {
        private const int MaxNoOfRetries = 3;
        private string ConnectionString { get; }
        private AsyncRetryPolicy RetryPolicy { get; }

        public QueryStoreClient(IOptions<ConnectionStrings> config, ILogger<QueryStoreClient> logger)
        {
            ConnectionString = config.Value.MongoDb;
            RetryPolicy = Policy
                .Handle<MongoException>()
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), MaxNoOfRetries)
                , (exception, timeSpan, retryCount, context) => {
                    logger.LogWarning($"Error executing Mongo Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                });
        }

        public Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId)
        {
            var key = QueryViewType.EmployerDashboard.GetIdValue(employerAccountId);

            return GetAsync<EmployerDashboard>(QueryViewType.EmployerDashboard.TypeName, key);
        }

        public Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn)
        {
            var key = QueryViewType.ProviderDashboard.GetIdValue(ukprn);

            return GetAsync<ProviderDashboard>(QueryViewType.ProviderDashboard.TypeName, key);
        }

        public Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference)
        {
            var key = QueryViewType.VacancyApplications.GetIdValue(vacancyReference);

            return GetAsync<VacancyApplications>(QueryViewType.VacancyApplications.TypeName, key);
        }

        public Task<BlockedProviderOrganisations> GetBlockedProviders()
        {
            return GetAsync<BlockedProviderOrganisations>(QueryViewType.BlockedProviderOrganisations.TypeName, QueryViewType.BlockedProviderOrganisations.GetIdValue());
        }

        public Task<BlockedEmployerOrganisations> GetBlockedEmployers()
        {
            return GetAsync<BlockedEmployerOrganisations>(QueryViewType.BlockedEmployerOrganisations.TypeName, QueryViewType.BlockedEmployerOrganisations.GetIdValue());
        }

        private IMongoDatabase GetDatabase()
        {
            const string RecruitDbName = "recruit";
            var settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

            var client = new MongoClient(settings);
            var database = client.GetDatabase(RecruitDbName);

            return database;
        }

        private IMongoCollection<T> GetCollection<T>()
        {
            const string CollectionName = "queryStore";
            var database = GetDatabase();
            var collection = database.GetCollection<T>(CollectionName);

            return collection;
        }

        private async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, typeName)
                        & filterBuilder.Eq(d => d.Id, key);

            var collection = GetCollection<T>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter).FirstOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }
    }
}