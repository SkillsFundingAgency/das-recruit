using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    internal sealed class MongoDbReferenceDataRepository : MongoDbCollectionBase, IReferenceDataReader, IReferenceDataWriter
    {
        private const string Database = "recruit";
        private const string Collection = "referenceData";
        private const string Id = "_id";
        private readonly IDictionary<Type, string> _itemIdLookup;
        private readonly ITimeProvider _timeProvider;

        public MongoDbReferenceDataRepository(ILogger<MongoDbReferenceDataRepository> logger, IOptions<MongoDbConnectionDetails> details, ITimeProvider timeProvider)
            : base(logger, Database, Collection, details)
        {
            _timeProvider = timeProvider;
            _itemIdLookup = BuildLookup();
        }

        public async Task<T> GetReferenceData<T>() where T : class, IReferenceDataItem
        {
            try
            {
                var id = _itemIdLookup[typeof(T)];

                var filter = Builders<BsonDocument>.Filter.Eq(Id, id);
                var options = new FindOptions<BsonDocument, T> { Limit = 1 };

                var collection = GetCollection<BsonDocument>();
                var result = await collection.FindAsync(filter, options);
                
                return result?.SingleOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentOutOfRangeException($"{typeof(T).Name} is not a recognised reference data type", ex);
            }
        }

        public Task UpsertReferenceData<T>(T referenceData) where T : class, IReferenceDataItem
        {
            var id = _itemIdLookup[typeof(T)];
            referenceData.Id = id;
            referenceData.LastUpdatedDate = _timeProvider.Now;
            
            var collection = GetCollection<T>();

            var filter = Builders<T>.Filter.Eq(Id, id);

            return RetryPolicy.ExecuteAsync(context => 
                collection.ReplaceOneAsync(
                    filter, 
                    referenceData, 
                    new UpdateOptions { IsUpsert = true }), 
                new Context(nameof(IReferenceDataWriter.UpsertReferenceData)));
        }

        private IDictionary<Type, string> BuildLookup()
        {
            return new Dictionary<Type, string> 
                {
                    { typeof(CandidateSkills), "CandidateSkills" },
                    { typeof(MinimumWages), "MinimumWageRanges" },
                    { typeof(BankHolidays), "BankHolidays" },
                    { typeof(Qualifications), "QualificationTypes"}
                };
        }
    }
}