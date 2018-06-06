using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    internal sealed class MongoDbReferenceDataRepository : MongoDbCollectionBase, IReferenceDataReader
    {
        private const string Database = "recruit";
        private const string Collection = "referenceData";

        private const string Id = "_id";
        private const string CandidateSkills = "CandidateSkills";

        public MongoDbReferenceDataRepository(ILogger<MongoDbReferenceDataRepository> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
        {
        }

        async Task<CandidateSkills> IReferenceDataReader.GetCandidateSkillsAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Eq(Id, CandidateSkills);
            var options = new FindOptions<BsonDocument, CandidateSkills> { Limit = 1 };

            var collection = GetCollection<BsonDocument>();
            var result = await collection.FindAsync(filter, options);

            return result?.SingleOrDefault();
        }
    }
}