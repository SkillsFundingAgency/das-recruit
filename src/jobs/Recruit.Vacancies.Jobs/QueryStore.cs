using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Jobs.Models;
using Esfa.Recruit.Vacancies.Jobs.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SFA.DAS.Apprenticeships.Api.Types;

namespace Esfa.Recruit.Vacancies.Jobs
{
    internal sealed class QueryStore : MongoDbCollectionBase, IUpdateQueryStore
    {
        private const string Database = "recruit";
        private const string Collection = "queryViews";

        public QueryStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        public async Task UpdateStandardsAndFrameworksAsyc(ApprenticeshipProgrammeView updatedList)
        {
            var collection = GetCollection<ApprenticeshipProgrammeView>();

            var filter = Builders<ApprenticeshipProgrammeView>.Filter.Eq(x => x.Id, updatedList.Id);
            var options = new UpdateOptions 
            {
                IsUpsert = true,
            };

            await collection.ReplaceOneAsync(filter, updatedList, options);
        }
    }
}