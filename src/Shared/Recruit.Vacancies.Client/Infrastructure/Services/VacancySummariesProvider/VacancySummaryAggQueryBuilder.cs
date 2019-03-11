using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using MongoDB.Bson;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public static class VacancySummaryAggQueryBuilder
    {
        private const string PathSpecifierFieldName = "path";
        private const string PreserveNullAndEmptyArraysSpecifierFieldName = "preserveNullAndEmptyArrays";
        private static readonly BsonArray _newApplicationReviewStatusClause = new BsonArray { "$appStatus", ApplicationReviewStatus.New.ToString() };

        private static readonly BsonArray _successfulApplicationReviewStatusClause = new BsonArray { "$appStatus", ApplicationReviewStatus.Successful.ToString() };

        private static readonly BsonArray _unsuccessfulApplicationReviewStatusClause = new BsonArray { "$appStatus", ApplicationReviewStatus.Unsuccessful.ToString() };

        private static readonly BsonDocument _lookupClause = new BsonDocument
        {
            {
                "$lookup",
                new BsonDocument
                {
                    { "from", MongoDbCollectionNames.ApplicationReviews },
                    { "localField", "vacancyReference" },
                    { "foreignField", "vacancyReference" },
                    { "as", "application" }
                }
            }
        };
        private static readonly BsonDocument _unwindClause = new BsonDocument
        {
            {
                "$unwind",
                new BsonDocument().Add(PathSpecifierFieldName, "$application")
                                .Add(PreserveNullAndEmptyArraysSpecifierFieldName, true)
            }
        };
        private static readonly BsonDocument _projectionOneClause = new BsonDocument
        {
            {
                "$project",
                new BsonDocument
                {
                    { "vacancyGuid", "$_id" },
                    { "vacancyReference", 1 },
                    { "title", 1 },
                    { "status", 1 },
                    { "appStatus", "$application.status" },
                    { "employerName", 1 },
                    { "createdDate", 1 },
                    { "closingDate", 1 },
                    { "applicationMethod", 1 },
                    { "programmeId", 1 }
                }
            }
        };
        private static readonly BsonDocument _projectionTwoClause = new BsonDocument
        {
            {
                "$project",
                new BsonDocument
                {
                    { "vacancyGuid", 1 },
                    { "vacancyReference", 1 },
                    { "title", 1 },
                    { "status", 1 },
                    { "employerName", 1 },
                    { "createdDate", 1 },
                    { "closingDate", 1 },
                    { "applicationMethod", 1 },
                    { "programmeId", 1 }
                }
                .Add("isNew", new BsonDocument()
                                .Add("$cond", new BsonDocument().Add("if", new BsonDocument().Add("$eq", _newApplicationReviewStatusClause))
                                                                .Add("then", 1)
                                                                .Add("else", 0)
                                    )
                    )
                .Add("isSuccessful", new BsonDocument()
                                        .Add("$cond", new BsonDocument().Add("if", new BsonDocument().Add("$eq", _successfulApplicationReviewStatusClause))
                                                                        .Add("then", 1)
                                                                        .Add("else", 0)
                                            )
                    )
                .Add("isUnsuccessful", new BsonDocument()
                                            .Add("$cond", new BsonDocument().Add("if", new BsonDocument().Add("$eq", _unsuccessfulApplicationReviewStatusClause))
                                                                            .Add("then", 1)
                                                                            .Add("else", 0)
                                                )
                    )
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
                            { "vacancyGuid", "$vacancyGuid" },
                            { "vacancyReference", "$vacancyReference" },
                            { "title", "$title" },
                            { "status", "$status" },
                            { "employerName", "$employerName" },
                            { "createdDate", "$createdDate" },
                            { "closingDate", "$closingDate" },
                            { "applicationMethod", "$applicationMethod" },
                            { "programmeId", "$programmeId" }
                        }
                    },
                    { "noOfNewApplications", new BsonDocument().Add("$sum", "$isNew") },
                    { "noOfSuccessfulApplications", new BsonDocument().Add("$sum", "$isSuccessful") },
                    { "noOfUnsuccessfulApplications", new BsonDocument().Add("$sum", "$isUnsuccessful") }
                }
            }
        };

        public static BsonDocument[] GetAggregateQueryPipeline(BsonDocument vacanciesMatchClause)
        {
            return new BsonDocument[]
            {
                vacanciesMatchClause,
                _lookupClause,
                _unwindClause,
                _projectionOneClause,
                _projectionTwoClause,
                //_groupClause
            };
        }
    }
}