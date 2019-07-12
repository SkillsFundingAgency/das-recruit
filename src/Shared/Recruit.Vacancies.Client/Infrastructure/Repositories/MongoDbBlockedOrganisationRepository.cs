using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbBlockedOrganisationRepository : MongoDbCollectionBase, IBlockedOrganisationRepository, IBlockedOrganisationQuery
    {
        public MongoDbBlockedOrganisationRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.ApplicationReviews, details)
        {
        }

        public Task CreateAsync(BlockedOrganisation organisation)
        {
            var collection = GetCollection<BlockedOrganisation>();
            return RetryPolicy.ExecuteAsync(_ =>
                collection.InsertOneAsync(organisation),
                new Context(nameof(CreateAsync)));
        }

        public async Task<BlockedOrganisation> GetAsync(Guid blockedOrganisationId)
        {
            var filter = Builders<BlockedOrganisation>.Filter.Eq(r => r.Id, blockedOrganisationId);
            var collection = GetCollection<BlockedOrganisation>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }

        public async Task<List<BlockedOrganisation>> GetByOrganisationIdAsync<T>(string organisationId)
        {
            var filter = Builders<BlockedOrganisation>.Filter.Eq(bo => bo.OrganisationId, organisationId);
            var collection = GetCollection<BlockedOrganisation>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                .ToListAsync(),
            new Context(nameof(GetByOrganisationIdAsync)));

            return result;
        }
    }
}