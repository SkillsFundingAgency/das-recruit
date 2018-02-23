using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class QueryStore : MongoDbCollectionBase, IQueryStoreReader, IQueryStoreWriter
    {
        private const string Database = "recruit";
        private const string Collection = "vacancies";

        public QueryStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.EmployerAccountId, employerAccountId);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);

            return result.ToEnumerable();
        }
    }
}