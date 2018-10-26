using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Holidays = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Quals = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Programmes = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BlockedEmployers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    internal sealed class MongoDbReferenceDataRepository : MongoDbCollectionBase, IReferenceDataReader, IReferenceDataWriter
    {
        private const string Id = "_id";
        private readonly IDictionary<Type, string> _itemIdLookup;
        private readonly ITimeProvider _timeProvider;

        public MongoDbReferenceDataRepository(ILogger<MongoDbReferenceDataRepository> logger, IOptions<MongoDbConnectionDetails> details, ITimeProvider timeProvider)
            : base(logger, MongoDbNames.RecruitDb, MongoDbCollectionNames.ReferenceData, details)
        {
            _timeProvider = timeProvider;
            _itemIdLookup = BuildLookup();
        }

        public async Task<T> GetReferenceData<T>() where T : class, IReferenceDataItem
        {
            try
            {
                var id = _itemIdLookup[typeof(T)];

                var filter = Builders<T>.Filter.Eq(Id, id);
                var options = new FindOptions<T> { Limit = 1 };

                var collection = GetCollection<T>();
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
                { typeof(Holidays.BankHolidays), "BankHolidays" },
                { typeof(Quals.Qualifications), "QualificationTypes" },
                { typeof(Programmes.ApprenticeshipProgrammes), "ApprenticeshipProgrammes" },
                { typeof(Profanities.ProfanityList), "Profanities" },
                { typeof(BannedPhrases.BannedPhraseList), "BannedPhrases" },
                { typeof(BlockedEmployers.BlockedEmployers), "BlockedEmployers" }
            };
        }
    }
}