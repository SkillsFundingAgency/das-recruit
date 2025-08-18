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
            { '$match' : { 'candidateApplicationReview.isWithdrawn' : { '$ne' : true } } },
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
                        'vacancyReference': '$vacancyReference',
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

        private readonly string _dashboardPipelineNoApplicationReview = @"[
            {
                '$project': {
                    'vacancyReference': 1
                }
            },
            {
                '$group': {
                    '_id': {
                        'vacancyReference': '$vacancyReference'    
                    }                    
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
                    'isTraineeship' :1,
                    'apprenticeshipType': 1
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
                    },
                    'apprenticeshipType': 1
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
                    'apprenticeshipType': 1,
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
                        'apprenticeshipType' : '$apprenticeshipType'
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

        private const string PipelineNoApplicationReview = @"[
    { '$sort' : { 'createdDate' : -1} },
    {
        '$project': {
            'vacancyGuid': '$_id',
            'searchField': 1,
            'vacancyReference': 1,
            'title': 1,
            'status': 1,
            'appStatus': '',
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
            'isApplicationWithdrawn': '',
            'dateSharedWithEmployer': '',
            'hasChosenProviderContactDetails' : 1,
            'hasSubmittedAdditionalQuestions' : 1,
            'isTraineeship' :1,
            'apprenticeshipType': 1
        }
    },
    {
        '$project': {
            'vacancyGuid': 1,
            'searchField': 1,
            'vacancyReference': 1,
            'title': 1,
            'status': 1,
            'appStatus': '',
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
            },
            'apprenticeshipType': 1
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
            'apprenticeshipType': 1,
            'isTraineeship': {
                '$cond': {
                    'if': {'$eq': [ '$vacancyType', 'Traineeship']},
                    'then': true,
                    'else': false
                }
            },
            'hasChosenProviderContactDetails' : 1,
            'hasSubmittedAdditionalQuestions' : 1,
            'isNew': 1,
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
                'hasSubmittedAdditionalQuestions' : '$hasSubmittedAdditionalQuestions',
                'apprenticeshipType': '$apprenticeshipType'
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

        private const string PipelineForCountMigration = @"[
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

        public static BsonDocument[] GetAggregateQueryPipeline(BsonDocument vacanciesMatchClause,
            int pageNumber,
            BsonDocument employerReviewMatch = null,
            BsonDocument vacancyRefMatch = null,
            BsonDocument searchMatch = null)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(PipelineNoApplicationReview);

            const int index = 1;
            if (employerReviewMatch != null)
            {
                pipeline.Insert(index, employerReviewMatch);
                
            }
            if (searchMatch != null)
            {
                pipeline.Insert(index, searchMatch);
            }

            if (vacancyRefMatch != null)
            {
                pipeline.Insert(index, vacancyRefMatch);
            }
            
            pipeline.Insert(pipeline.Count, new BsonDocument { { "$skip", (pageNumber - 1) * 100 } });
            pipeline.Insert(pipeline.Count, new BsonDocument { { "$limit", 100 } });
            
            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }

        public static BsonDocument[] GetAggregateQueryPipelineDocumentCount(BsonDocument vacanciesMatchClause,
            BsonDocument employerReviewMatch,
            BsonDocument searchMatch)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(PipelineForCountMigration);
            
            if (employerReviewMatch != null)
            {
                pipeline.Insert(0, employerReviewMatch);    
            }
            if (searchMatch != null)
            {
                pipeline.Insert(0, searchMatch);    
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
            var pipeline = BsonSerializer.Deserialize<BsonArray>(_dashboardPipelineNoApplicationReview);

            if (employerReviewMatch != null)
            {
                pipeline.Insert(0, employerReviewMatch);
            }

            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }
    }
}