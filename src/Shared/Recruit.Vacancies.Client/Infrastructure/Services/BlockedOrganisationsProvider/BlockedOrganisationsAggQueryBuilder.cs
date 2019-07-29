using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MongoDB.Bson;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.BlockedOrganisationsProvider
{
    public static class BlockedOrganisationsAggQueryBuilder
    {
        private static readonly BsonDocument _sortClause = new BsonDocument
        {
            {
                "$sort",
                new BsonDocument
                {
                    { "updatedDate", -1 }
                }
            }
        };

        private static readonly BsonDocument _groupClause = new BsonDocument
        {
            {
                "$group",
                new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { "organisationId", "$organisationId" }
                        }
                    },
                    { "organisationId", new BsonDocument().Add("$first", "$organisationId") },
                    { "updatedDate", new BsonDocument().Add("$first", "$updatedDate") },
                    { "updatedByUser", new BsonDocument().Add("$first", "$updatedByUser.name") },
                    { "organisationType", new BsonDocument().Add("$first", "$organisationType") },
                    { "blockedStatus", new BsonDocument().Add("$first", "$blockedStatus") },
                }
            }
        };

        private static readonly BsonDocument _matchBlockedClause = new BsonDocument
        {
            {
                "$match",
                new BsonDocument
                {
                    { "blockedStatus", BlockedStatus.Blocked.ToString() }
                }
            }
        };

        private static readonly BsonDocument _matchEmployerOrganisationTypeClause = new BsonDocument
        {
            {
                "$match",
                new BsonDocument
                {
                    { "organisationType", OrganisationType.Employer.ToString() }
                }
            }
        };

        private static readonly BsonDocument _matchProviderOrganisationTypeClause = new BsonDocument
        {
            {
                "$match",
                new BsonDocument
                {
                    { "organisationType", OrganisationType.Provider.ToString() }
                }
            }
        };

        private static readonly BsonDocument _projectClause = new BsonDocument
        {
            {
                "$project",
                new BsonDocument
                {
                    { "_id", 0 },
                    { "blockedOrganisationId", "$organisationId" },
                    { "blockedDate", "$updatedDate" },
                    { "blockedByUser", "$updatedByUser" }
                }
            }
        };

        public static BsonDocument[] GetBlockedProvidersAggregateQueryPipeline()
        {
            return new BsonDocument[]
            {
                _sortClause,
                _groupClause,
                _matchBlockedClause,
                _matchProviderOrganisationTypeClause,
                _projectClause
            };
        }

        public static BsonDocument[] GetBlockedEmployersAggregateQueryPipeline()
        {
            return new BsonDocument[]
            {
                _sortClause,
                _groupClause,
                _matchBlockedClause,
                _matchEmployerOrganisationTypeClause,
                _projectClause
            };
        }
    }
}