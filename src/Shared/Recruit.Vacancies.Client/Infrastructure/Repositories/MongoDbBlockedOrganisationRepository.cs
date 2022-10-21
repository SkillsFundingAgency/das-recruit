using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.BlockedOrganisationsProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbBlockedOrganisationRepository : MongoDbCollectionBase, IBlockedOrganisationRepository, IBlockedOrganisationQuery
    {
        public MongoDbBlockedOrganisationRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.BlockedOrganisations, details)
        {
        }

        public Task CreateAsync(BlockedOrganisation organisation)
        {
            var collection = GetCollection<BlockedOrganisation>();
            return RetryPolicy.Execute(_ =>
                collection.InsertOneAsync(organisation),
                new Context(nameof(CreateAsync)));
        }

        public async Task<BlockedOrganisation> GetByOrganisationIdAsync(string organisationId)
        {
            var builder = Builders<BlockedOrganisation>.Filter;
            var filter = builder.Eq(bo => bo.OrganisationId, organisationId);
            var collection = GetCollection<BlockedOrganisation>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter)
                    .SortByDescending(bo => bo.UpdatedDate).FirstOrDefaultAsync(),
            new Context(nameof(GetByOrganisationIdAsync)));

            return result;
        }

        public async Task<List<BlockedOrganisationSummary>> GetAllBlockedProvidersAsync()
        {
            var collection = GetCollection<BlockedOrganisation>();
            var result = await RetryPolicy.Execute(async _ =>
                                                        {
                                                            var pipeline = BlockedOrganisationsAggQueryBuilder.GetBlockedProvidersAggregateQueryPipeline();
                                                            var mongoQuery = pipeline.ToJson();
                                                            var aggResults = await collection.AggregateAsync<BlockedOrganisationSummary>(pipeline);
                                                            return await aggResults.ToListAsync();
                                                        },
                                                        new Context(nameof(GetAllBlockedProvidersAsync)));

            return result.ToList();
        }

        public async Task<List<BlockedOrganisationSummary>> GetAllBlockedEmployersAsync()
        {
            var collection = GetCollection<BlockedOrganisation>();
            var result = await RetryPolicy.Execute(async _ =>
                                                        {
                                                            var pipeline = BlockedOrganisationsAggQueryBuilder.GetBlockedEmployersAggregateQueryPipeline();
                                                            var mongoQuery = pipeline.ToJson();
                                                            var aggResults = await collection.AggregateAsync<BlockedOrganisationSummary>(pipeline);
                                                            return await aggResults.ToListAsync();
                                                        },
                                                        new Context(nameof(GetAllBlockedProvidersAsync)));

            return result.ToList();
        }
    }
}