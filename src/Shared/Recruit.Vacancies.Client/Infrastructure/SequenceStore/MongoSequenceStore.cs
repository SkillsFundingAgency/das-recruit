using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore
{
    internal sealed class MongoSequenceStore : MongoDbCollectionBase, IGenerateVacancyNumbers
    {
        private const string Database = "recruit";
        private const string Collection = "sequences";
        private const string VacancyNumberSequenceName = "Sequence_Vacancy";

        public MongoSequenceStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        public async Task<long> GenerateAsync()
        {
            var collection = GetCollection<Sequence>();
            var filter = Builders<Sequence>.Filter.Eq(x => x.Id, VacancyNumberSequenceName);
            var update = Builders<Sequence>.Update.Inc(x => x.LastValue, 1);
            var options = new FindOneAndUpdateOptions<Sequence> { ReturnDocument = ReturnDocument.After };
            
            var newSequence = await collection.FindOneAndUpdateAsync(filter, update, options);

            return newSequence.LastValue;
        }
    }
}