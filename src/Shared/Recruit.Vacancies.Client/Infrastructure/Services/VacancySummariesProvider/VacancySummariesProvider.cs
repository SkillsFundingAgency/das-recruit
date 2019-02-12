using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal sealed class VacancySummariesProvider : MongoDbCollectionBase, IVacancySummariesProvider
    {
        private const string PathSpecifierFieldName = "path";

        private const string PreserveNullAndEmptyArraysSpecifierFieldName = "preserveNullAndEmptyArrays";

        public VacancySummariesProvider(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
        }

        public async Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountAsync(string employerAccountId)
        {
            var match = new BsonDocument
            {
                {
                    "$match",
                    new BsonDocument
                    {
                        { "employerAccountId", employerAccountId },
                        { "ownerType", OwnerType.Employer.ToString() },
                        { "isDeleted", false }
                    }
                }
            };

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn)
        {
            var match = new BsonDocument
            {
                {
                    "$match",
                    new BsonDocument
                    {
                        { "trainingProvider.ukprn", ukprn },
                        { "ownerType", OwnerType.Provider.ToString() },
                        { "isDeleted", false }
                    }
                }
            };

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match);

            return await RunAggPipelineQuery(aggPipeline);
        }

        private async Task<IList<VacancySummary>> RunAggPipelineQuery(BsonDocument[] pipeline)
        {
            var mongoQuery = pipeline.ToJson();

            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);

            var vacancySummaries = await RetryPolicy.ExecuteAsync(async context =>
                                                                    {
                                                                        var aggResults = await collection.AggregateAsync<VacancySummaryAggQueryResponseDto>(pipeline);
                                                                        return await aggResults.ToListAsync();
                                                                    },
                                                                    new Context(nameof(RunAggPipelineQuery)));

            return vacancySummaries.Select(VacancySummaryMapper.MapFromVacancySummaryAggQueryResponseDto).ToList();
        }
    }
}