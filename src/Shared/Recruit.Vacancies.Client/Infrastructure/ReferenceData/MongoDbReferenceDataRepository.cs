using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
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
        private const string CandidateSkills = "CandidateSkills";
        private const string BankHolidays = "BankHolidays";

        private ITimeProvider _timeProvider;

        public MongoDbReferenceDataRepository(ILogger<MongoDbReferenceDataRepository> logger, IOptions<MongoDbConnectionDetails> details, ITimeProvider timeProvider)
            : base(logger, Database, Collection, details)
        {
            _timeProvider = timeProvider;
        }

        async Task<CandidateSkills> IReferenceDataReader.GetCandidateSkillsAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq(Id, CandidateSkills);
            var options = new FindOptions<BsonDocument, CandidateSkills> { Limit = 1 };

            var collection = GetCollection<BsonDocument>();
            var result = await collection.FindAsync(filter, options);

            return result?.SingleOrDefault();
        }

        async Task<BankHolidays> IReferenceDataReader.GetBankHolidaysAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq(Id, BankHolidays);
            var options = new FindOptions<BsonDocument, BankHolidays> { Limit = 1 };

            var collection = GetCollection<BsonDocument>();
            var result = await collection.FindAsync(filter, options);

            return result?.SingleOrDefault();
        }

        public Task UpsertBankHolidays(BankHolidays bankHolidays)
        {
            bankHolidays.Id = BankHolidays;
            bankHolidays.LastUpdatedDate = _timeProvider.Now;
            
            var collection = GetCollection<BankHolidays>();

            var filter = Builders<BankHolidays>.Filter.Eq(Id, BankHolidays);

            return RetryPolicy.ExecuteAsync(context => 
                collection.ReplaceOneAsync(
                    filter, 
                    bankHolidays, 
                    new UpdateOptions { IsUpsert = true }), 
                new Context(nameof(UpsertBankHolidays)));
        }
    }
}