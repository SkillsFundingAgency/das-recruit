using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore
{
    internal sealed class MongoSequenceStore : MongoDbCollectionBase, IGenerateVacancyNumbers
    {
        private const string Database = "recruit";
        private const string Collection = "sequences";
        private const string VacancyNumberSequenceName = "Sequence_Vacancy";

        public MongoSequenceStore(ILogger<MongoSequenceStore> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
        {
        }

        public async Task<long> GenerateAsync()
        {
            var collection = GetCollection<Sequence>();
            var filter = Builders<Sequence>.Filter.Eq(x => x.Id, VacancyNumberSequenceName);
            var update = Builders<Sequence>.Update.Inc(x => x.LastValue, 1);
            var options = new FindOneAndUpdateOptions<Sequence> { ReturnDocument = ReturnDocument.After };
            
            var newSequence = await RetryPolicy.ExecuteAsync(() => collection.FindOneAndUpdateAsync(filter, update, options));

            if (newSequence == null)
            {
                throw new InfrastructureException($"Sequence not found in store: {VacancyNumberSequenceName}.");
            }

            return newSequence.LastValue;
        }
    }
}