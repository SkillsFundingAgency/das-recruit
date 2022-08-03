using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public static class VacancySummaryAggQueryBuilder
    {
        private const string DashboardPipeline = @"[
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
                    'vacancyType': 1,
                    'isTraineeship' :1,
                    'closingDate':1
                }
            },
            {
                '$project': {
                    'status': 1,
                    'appStatus': { '$cond' : [ { '$eq': ['$isApplicationWithdrawn', true] }, 'withdrawn', '$appStatus' ]},
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
                            'if': {'$lte':[{$dateDiff: {
                                                  startDate: new Date(),
                                                  endDate:'$closingDate' ,
                                                  unit: 'day'
                                               }},5]},
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
                    'isUnsuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Unsuccessful']},
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
                        'closingSoon' : '$closingSoon',
                    },
                    'statusCount' : { '$sum' : 1 },
                    'noOfNewApplications': {
                        '$sum': '$isNew'
                    },
                    'noOfSuccessfulApplications': {
                        '$sum': '$isSuccessful'
                    },
                    'noOfUnsuccessfulApplications': {
                        '$sum': '$isUnsuccessful'
                    }
                }
            }
        ]";
        
        private const string Pipeline = @"[
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
                    'isTraineeship' :1
                }
            },
            {
                '$project': {
                    'vacancyGuid': 1,
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
                    'isUnsuccessful': {
                        '$cond': {
                            'if': {'$eq': [ '$appStatus', 'Unsuccessful']},
                            'then': 1,
                            'else': 0
                        }
                    }
                }
            },
            {
                '$group': {
                    '_id': {
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
                        'isTraineeship': '$isTraineeship'
                    },
                    'noOfNewApplications': {
                        '$sum': '$isNew'
                    },
                    'noOfSuccessfulApplications': {
                        '$sum': '$isSuccessful'
                    },
                    'noOfUnsuccessfulApplications': {
                        '$sum': '$isUnsuccessful'
                    }
                }
            }
        ]";

        public static BsonDocument[] GetAggregateQueryPipeline(BsonDocument vacanciesMatchClause)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(Pipeline);
            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }
        
        public static BsonDocument[] GetAggregateQueryPipelineDashboard(BsonDocument vacanciesMatchClause)
        {
            var pipeline = BsonSerializer.Deserialize<BsonArray>(DashboardPipeline);
            pipeline.Insert(0, vacanciesMatchClause);

            var pipelineDefinition = pipeline.Values.Select(p => p.ToBsonDocument()).ToArray();

            return pipelineDefinition;
        }
    }
}