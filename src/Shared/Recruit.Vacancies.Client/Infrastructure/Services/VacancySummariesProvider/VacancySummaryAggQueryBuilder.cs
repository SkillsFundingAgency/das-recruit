using System;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public class VacancySummaryAggQueryBuilder
    {
        private string DashboardApplicationsPipeline = @"[
            {
                '$lookup': {
                    'from': 'applicationReviews',
                    'localField': 'vacancyReference',
                    'foreignField': 'vacancyReference',
                    'as': 'candidateApplicationReview'
                }
            },
            {
                '$unwind': {
                    'path': '$candidateApplicationReview',
                    'preserveNullAndEmptyArrays': true
                }
            },
            {
                '$project': {
                    'status': 1,
                    'appStatus': '$candidateApplicationReview.status',
                    'isApplicationWithdrawn': '$candidateApplicationReview.isWithdrawn',
                    'dateSharedWithEmployer': '$candidateApplicationReview.dateSharedWithEmployer',
                    'vacancyType': 1,
                    'isTraineeship' :1,
                    'closingDate' : 1
                }
            },
            {
                '$project': {
                    'status': 1,
                    'appStatus': { '$cond' : [ { '$eq': ['$isApplicationWithdrawn', true] }, 'withdrawn', '$appStatus' ]},
                    'vacancyType': 1,
                    'closingDate' : 1,
                    'dateSharedWithEmployer': 1,
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    }
                }
            },
            { 
                '$match' : { 
                    'appStatus':{ $ne: 'withdrawn'} 
                            }
            },
            {
                '$project': {
                    'status': 1,
                    'vacancyType': 1,
                    'dateSharedWithEmployer': 1,
                    'closingSoon' : {
                        '$cond': {
                            'if': {'$lte':[
                                '$closingDate',ISODate('" + DateTime.UtcNow.AddDays(5).ToString("o", CultureInfo.InvariantCulture) + @"')
                            ]},
                                
                            'then': {
                                '$cond':{
                                    'if': {'$eq': [ '$status', 'Live']},
                                        'then': true,
                                        'else': false
                                }
                            },
                            'else': false
                        }
                    },
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    },
                    'isNew': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'New']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isSuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Successful']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isEmployerReviewed': {
                        '$cond': {
                            'if': {
                                '$or': [
                                    { '$eq': ['$appStatus', 'EmployerInterviewing'] },
                                    { '$eq': ['$appStatus', 'EmployerUnsuccessful'] }
                                ]
                            },
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isUnsuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Unsuccessful']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isShared': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Shared']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isSharedWithEmployer': {
                        '$cond': {
                            'if': {'$gte': [ '$dateSharedWithEmployer', '1900-01-01T01:00:00.389Z'] },
                            'then': 1,
                            'else': 0
                        }
                    }
                }
            },
            {
                '$group': {
                    '_id': {
                        'status':'$status',
                        'isTraineeship' : '$isTraineeship',
                        'closingSoon' : '$closingSoon'    
                    },
                    'noOfNewApplications': {
                        '$sum': '$isNew'
                    },
                    'noOfSuccessfulApplications': {
                        '$sum': '$isSuccessful'
                    },
                    'noOfEmployerReviewedApplications': {
                        '$sum': '$isEmployerReviewed'
                    },
                    'noOfUnsuccessfulApplications': {
                        '$sum': '$isUnsuccessful'
                    },
                    'noOfSharedApplications': {
                        '$sum': '$isShared'
                    },
                    'noOfAllSharedApplications': {
                        '$sum': '$isSharedWithEmployer'
                    },
                    'statusCount' : { '$sum' : 1 }
                    
                }
            }
        ]";

        private string DashboardNoApplicationCountMatchClause = @"{ '$match' :{ 'candidateApplicationReview' : null, 'status':'Live'  }}";

        private string DashboardPipeline = @"[
            {
                '$project': {
                    'status': 1,
                    'vacancyType': 1,
                    'isTraineeship' :1,
                    'closingDate':1
                }
            },
            {
                '$project': {
                    'status': 1,
                    'vacancyType': 1,
                    'closingDate':1,
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    }
                }
            },
            {
                '$project': {
                    'status': 1,
                    'vacancyType': 1,
                    'closingDate':1,
                    'closingSoon' : {
                        '$cond': {
                            'if': {'$lte':[
                                '$closingDate',ISODate('" + DateTime.UtcNow.AddDays(5).ToString("o", CultureInfo.InvariantCulture) + @"')
                            ]},
                                
                            'then': {
                                '$cond':{
                                    'if': {'$eq': [ '$status', 'Live']},
                                        'then': true,
                                        'else': false
                                }
                            },
                            'else': false
                        }
                    },
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    }
                }
            },
            {
                '$group': {
                    '_id': {
                        'status':'$status',
                        'isTraineeship' : '$isTraineeship',
                        'closingSoon' : '$closingSoon'                        
                    },
                    'statusCount' : { '$sum' : 1 }
                    
                }
            }
        ]";

        private const string Pipeline = @"[
            { '$sort' : { 'createdDate' : -1} },
            {
                '$lookup': {
                    'from': 'applicationReviews',
                    'localField': 'vacancyReference',
                    'foreignField': 'vacancyReference',
                    'as': 'candidateApplicationReview'
                }
            },        
            {
                '$unwind': {
                    'path': '$candidateApplicationReview',
                    'preserveNullAndEmptyArrays': true
                }
            },    
            {
                '$project': {
                    'vacancyGuid': '$_id',
                    'searchField': 1,
                    'vacancyReference': 1,
                    'title': 1,
                    'status': 1,
                    'appStatus': '$candidateApplicationReview.status',
                    'legalEntityName': 1,
                    'employerAccountId': 1,
                    'employerName': 1,
                    'employerDescription': 1,
                    'ukprn': '$trainingProvider.ukprn',
                    'createdDate': 1,
                    'closingDate': 1,
                    'startDate': 1,
                    'closedDate': 1,
                    'closureReason': 1,
                    'applicationMethod': 1,
                    'programmeId': 1,
                    'duration': '$wage.duration',
                    'durationUnit': '$wage.durationUnit',
                    'transferInfoUkprn': '$transferInfo.ukprn',
                    'transferInfoProviderName': '$transferInfo.providerName',
                    'transferInfoTransferredDate': '$transferInfo.transferredDate',
                    'transferInfoReason': '$transferInfo.reason',
                    'trainingProviderName': '$trainingProvider.name',
                    'vacancyType': 1,
                    'isApplicationWithdrawn': '$candidateApplicationReview.isWithdrawn',
                    'dateSharedWithEmployer': '$candidateApplicationReview.dateSharedWithEmployer',
                    'hasChosenProviderContactDetails' : 1,
                    'hasSubmittedAdditionalQuestions' : 1,
                    'isTraineeship' :1
                }
            },
            {
                '$project': {
                    'vacancyGuid': 1,
                    'searchField': 1,
                    'vacancyReference': 1,
                    'title': 1,
                    'status': 1,
                    'appStatus': { '$cond' : [ { '$eq': ['$isApplicationWithdrawn', true] }, 'withdrawn', '$appStatus' ]},
                    'legalEntityName': 1,
                    'employerAccountId': 1,
                    'employerName': 1,
                    'employerDescription': 1,
                    'ukprn': 1,
                    'createdDate': 1,
                    'closingDate': 1,
                    'startDate': 1,
                    'closedDate': 1,
                    'closureReason': 1,
                    'applicationMethod': 1,
                    'programmeId': 1,
                    'duration': 1,
                    'durationUnit': 1,
                    'transferInfoUkprn': 1,
                    'transferInfoProviderName': 1,
                    'transferInfoTransferredDate': 1,
                    'transferInfoReason': 1,
                    'trainingProviderName': 1,
                    'vacancyType': 1,
                    'dateSharedWithEmployer': 1,
                    'hasChosenProviderContactDetails' : 1,
                    'hasSubmittedAdditionalQuestions' : 1,
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    }
                }
            },
            {
                '$project': {
                    'vacancyGuid': 1,
                    'searchField': 1,
                    'vacancyReference': 1,
                    'title': 1,
                    'status': 1,
                    'legalEntityName': 1,
                    'employerAccountId': 1,
                    'employerName': 1,
                    'employerDescription': 1,
                    'ukprn': 1,
                    'createdDate': 1,
                    'closingDate': 1,
                    'startDate': 1,
                    'closedDate': 1,
                    'closureReason': 1,
                    'applicationMethod': 1,
                    'programmeId': 1,
                    'duration': 1,
                    'durationUnit': 1,
                    'transferInfoUkprn': 1,
                    'transferInfoProviderName': 1,
                    'transferInfoTransferredDate': 1,
                    'transferInfoReason': 1,
                    'trainingProviderName': 1,
                    'vacancyType': 1,
                    'isTraineeship': {
                        '$cond': {
                            'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                            'then': true,
                            'else': false
                        }
                    },
                    'hasChosenProviderContactDetails' : 1,
                    'hasSubmittedAdditionalQuestions' : 1,
                    'isNew': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'New']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isSuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Successful']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isEmployerReviewed': {
                        '$cond': {
                            'if': {
                                '$or': [
                                    { '$eq': ['$appStatus', 'EmployerInterviewing'] },
                                    { '$eq': ['$appStatus', 'EmployerUnsuccessful'] }
                                ]
                            },
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isUnsuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Unsuccessful']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isShared': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Shared']},
                            'then': 1,
                            'else': 0
                        }
                    },
                    'isSharedWithEmployer': {
                        '$cond': {
                            'if': {'$gte': [ '$dateSharedWithEmployer', '1900-01-01T01:00:00.389Z'] },
                            'then': 1,
                            'else': 0
                        }
                    }
                }
            },
            {
                '$group': {
                    '_id': {
                        'searchField':{$toLower: { $concat: [ '$title', '|', {$ifNull:['$legalEntityName','']},'|','VAC',{$toString: {$ifNull:['$vacancyReference','']}} ] }},
                        'vacancyGuid': '$vacancyGuid',
                        'vacancyReference': '$vacancyReference',
                        'title': '$title',
                        'status': '$status',
                        'legalEntityName': '$legalEntityName',
                        'employerAccountId': '$employerAccountId',
                        'employerName': '$employerName',
                        'employerDescription': '$employerDescription',
                        'ukprn': '$ukprn',
                        'createdDate': '$createdDate',
                        'closingDate': '$closingDate',
                        'startDate': '$startDate',
                        'closedDate': '$closedDate',
                        'closureReason': '$closureReason',
                        'applicationMethod': '$applicationMethod',
                        'programmeId': '$programmeId',
                        'duration': '$duration',
                        'durationUnit': '$durationUnit',
                        'transferInfoUkprn': '$transferInfoUkprn',
                        'transferInfoProviderName': '$transferInfoProviderName',
                        'transferInfoTransferredDate': '$transferInfoTransferredDate',
                        'transferInfoReason': '$transferInfoReason',
                        'trainingProviderName': '$trainingProviderName',
                        'vacancyType': '$vacancyType',
                        'isTraineeship': '$isTraineeship',
                        'hasChosenProviderContactDetails' : '$hasChosenProviderContactDetails',
                        'hasSubmittedAdditionalQuestions' : '$hasSubmittedAdditionalQuestions'
                    },
                    'noOfNewApplications': {
                        '$sum': '$isNew'
                    },
                    'noOfSuccessfulApplications': {
                        '$sum': '$isSuccessful'
                    },
                    'noOfEmployerReviewedApplications': {
                        '$sum': '$isEmployerReviewed'
                    },
                    'noOfUnsuccessfulApplications': {
                        '$sum': '$isUnsuccessful'
                    },
                    'noOfSharedApplications': {
                        '$sum': '$isShared'
                    },
                    'noOfAllSharedApplications': {
                        '$sum': '$isSharedWithEmployer'
                    },
                    'noOfApplications': {
                         '$sum' :{'$add': ['$isNew','$isUnsuccessful','$isSuccessful'] }
                    }
                }
            }
        ]";
        
        
        private const string PipelineForCount = @"[
            {
                '$lookup': {
                    'from': 'applicationReviews',
                    'localField': 'vacancyReference',
                    'foreignField': 'vacancyReference',
                    'as': 'candidateApplicationReview'
                }
            },        
            {
                '$unwind': {
                    'path': '$candidateApplicationReview',
                    'preserveNullAndEmptyArrays': true
                }
            },    
            {
                '$project': {
                    'vacancyGuid': '$_id',
                    'vacancyReference': 1
                }
            },
            {
                '$project': {
                    'vacancyGuid': 1,
                    'vacancyReference': 1
                }
            },
            {
                '$group': {
                    '_id': {
                        'vacancyGuid': '$vacancyGuid',
                        'vacancyReference': '$vacancyReference'
                }}
            }
        ]";


        public static BsonDocument[] GetAggregateQueryPipeline(BsonDocument vacanciesMatchClause, int pageNumber, BsonDocument secondaryMatch, BsonDocument employerReviewMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(Pipeline);
            
            if (employerReviewMatch != null)
            {
                pipeline.Insert(3, employerReviewMatch);
                
            }
            if (secondaryMatch != null)
            {
                pipeline.Insert(3, secondaryMatch);
            }
            
            pipeline.Insert(pipeline.Count, new BsonDocument { { "$skip", (pageNumber - 1) * 25 } });
            pipeline.Insert(pipeline.Count, new BsonDocument { { "$limit", 25 } });
            
            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

        public static BsonDocument[] GetAggregateQueryPipelineDocumentCount(BsonDocument vacanciesMatchClause, BsonDocument secondaryMatch,BsonDocument employerReviewMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(PipelineForCount);
            pipeline.Insert(2, secondaryMatch);
            if (employerReviewMatch != null)
            {
                pipeline.Insert(2, employerReviewMatch);    
            }
            pipeline.Insert(pipeline.Count, new BsonDocument { { "$count", "total" } });
            
            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

        public BsonDocument[] GetAggregateQueryPipelineDashboard(BsonDocument vacanciesMatchClause, BsonDocument employerReviewMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(DashboardPipeline);
            if (employerReviewMatch != null)
            {
                pipeline.Insert(0, employerReviewMatch);
            }
            pipeline.Insert(0, vacanciesMatchClause);
            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

        public BsonDocument[] GetAggregateQueryPipelineDashboardApplications(BsonDocument vacanciesMatchClause, BsonDocument employerReviewMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(DashboardApplicationsPipeline);
            if (employerReviewMatch != null)
            {
                pipeline.Insert(0, employerReviewMatch);
            }
            pipeline.Insert(0, vacanciesMatchClause);
            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

        public BsonDocument[] GetAggregateQueryPipelineVacanciesClosingSoonDashboard(BsonDocument vacanciesMatchClause,
            BsonDocument employerReviewMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(DashboardApplicationsPipeline);
            var insertLine = 3;
            if (employerReviewMatch != null)
            {
                pipeline.Insert(0, employerReviewMatch);
                insertLine++;
            }
            pipeline.Insert(0, vacanciesMatchClause);

            var matchPipeline = BsonSerializer.Deserialize<BsonDocument>(DashboardNoApplicationCountMatchClause);

            pipeline.Insert(insertLine, matchPipeline);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

    }
}