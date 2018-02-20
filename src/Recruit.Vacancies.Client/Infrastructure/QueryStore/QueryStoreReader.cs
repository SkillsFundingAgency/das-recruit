using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using Esfa.Recruit.Storage.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public class QueryStoreReader : MongoDbCollectionBase, IQueryStoreReader
    {
        private const string Database = "recruit";
        private const string Collection = "vacancies";

        public QueryStoreReader(IOptions<MongoDbConnectionDetails> details) : base(Database, Collection, details)
        {
        }

        public async Task<Vacancy> GetVacancyForEditAsync(Guid vacancyId)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancyId);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.Single();
        }
    }
}