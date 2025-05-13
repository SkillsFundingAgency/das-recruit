using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly IVacancyTaskListStatusService _vacancyTaskListStatusService;
        private const string TransferInfoUkprn = "transferInfo.ukprn";
        private const string TransferInfoReason = "transferInfo.reason";
        private const int ClosingSoonDays = 5;

        public VacancySummariesProvider(
            ILoggerFactory loggerFactory, 
            IOptions<MongoDbConnectionDetails> details, 
            IVacancyTaskListStatusService vacancyTaskListStatusService)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
            _vacancyTaskListStatusService = vacancyTaskListStatusService;
        }
       
        public async Task<VacancyDashboard> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn)
        {
            var bsonArray = new BsonArray
            {
                VacancyType.Apprenticeship.ToString(),
                BsonNull.Value
            };

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, null, null, bsonArray)
                }
            };
            var applicationsMatch  = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, null, FilteringOptions.Dashboard, bsonArray)
                }
            }; 
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(applicationsMatch);
            var closingSoonAggPipeline = builder.GetAggregateQueryPipelineVacanciesClosingSoonDashboard(applicationsMatch);
            var dashboardValuesTask =  RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);
            var closingSoonDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(closingSoonAggPipeline);

            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask, closingSoonDashboardValuesTask);
            
            return new VacancyDashboard
            {
                VacancyStatusDashboard = dashboardValuesTask.Result,
                VacancyApplicationsDashboard = applicationDashboardValuesTask.Result,
                VacanciesClosingSoonWithNoApplications = closingSoonDashboardValuesTask.Result.FirstOrDefault(c => c.ClosingSoon)?.StatusCount ?? 0
            };
        }
        public async Task<VacancyDashboard> GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(string employerAccountId)
        {
            var bsonArray = new BsonArray
            {
                VacancyType.Apprenticeship.ToString(),
                BsonNull.Value
            };
            
            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null, employerAccountId, null, bsonArray)
                }
            };
            var employerReviewMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildEmployerReviewMatch()
                }
            };
            var liveVacanciesMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildSharedApplicationsVacanciesMatch()
                }
            };
            var applicationsMatch  = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null, employerAccountId, FilteringOptions.Dashboard, bsonArray)
                }
            }; 
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match,employerReviewMatch);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(applicationsMatch, employerReviewMatch);
            var sharedApplicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(match, liveVacanciesMatch);
            var closingSoonAggPipeline = builder.GetAggregateQueryPipelineVacanciesClosingSoonDashboard(applicationsMatch, employerReviewMatch);
            
            var dashboardValuesTask = RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);
            var sharedApplicationDashboardValuesTask = RunSharedApplicationsDashboardAggPipelineQuery(sharedApplicationAggPipeline);
            var closingSoonDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(closingSoonAggPipeline);
            
            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask, sharedApplicationDashboardValuesTask, closingSoonDashboardValuesTask);
            return new VacancyDashboard
            {
                VacancyStatusDashboard = dashboardValuesTask.Result,
                VacancyApplicationsDashboard = applicationDashboardValuesTask.Result,
                VacancySharedApplicationsDashboard = sharedApplicationDashboardValuesTask.Result,
                VacanciesClosingSoonWithNoApplications = closingSoonDashboardValuesTask.Result.FirstOrDefault()?.StatusCount ?? 0
            };
        }
        
        public async Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, int page, FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                VacancyType.Apprenticeship.ToString(),
                BsonNull.Value
            };
            
            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn,string.Empty, status, bsonArray)
                }
            };
            var secondaryMath = new BsonDocument
            {
                {
                    "$match",
                    BuildSecondaryBsonDocumentFilter(status, searchTerm)
                }
            };
            
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match, page,secondaryMath);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountId(string employerAccountId, int page,
            FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                VacancyType.Apprenticeship.ToString(),
                BsonNull.Value
            };
            

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null,employerAccountId, status, bsonArray)
                }
            };
            var employerReviewMatch = (status != FilteringOptions.NewSharedApplications && status != FilteringOptions.AllSharedApplications )?
                new BsonDocument
                {
                    {
                        "$match",
                        BuildEmployerReviewMatch()
                    }
                } : new BsonDocument
                {
                    {
                        "$match",
                        BuildSharedApplicationsVacanciesMatch()
                    }
                };
            var secondaryMath = new BsonDocument
            {
                {
                    "$match",
                    BuildSecondaryBsonDocumentFilter(status, searchTerm)
                }
            };
            
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match, page,secondaryMath, employerReviewMatch);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn)
        {
            var builder = Builders<VacancyTransferInfo>.Filter;
            var filter = builder.Eq(TransferInfoUkprn, ukprn) &
                         builder.Eq(TransferInfoReason, TransferReason.EmployerRevokedPermission.ToString()) &
                         builder.In("VacancyType",new []{"Apprenticeship", null});

            var collection = GetCollection<VacancyTransferInfo>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Find(filter)
                        .Project<VacancyTransferInfo>(GetProjection<VacancyTransferInfo>())
                        .ToListAsync(),
                new Context(nameof(GetTransferredFromProviderAsync)));

            return result.Select(v => v.TransferInfo).ToList();
        }

        public async Task<long> VacancyCount(long? ukprn, string employerAccountId, FilteringOptions? filteringOptions, string searchTerm, OwnerType ownerType)
        {
            var bsonArray = new BsonArray
            {
                VacancyType.Apprenticeship.ToString(),
                BsonNull.Value
            };
            

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, employerAccountId, filteringOptions, bsonArray)
                }
            };
            var secondaryMath = new BsonDocument
            {
                {
                    "$match",
                    BuildSecondaryBsonDocumentFilter(filteringOptions, searchTerm)
                }
            };
            var employerReviewMatch = (filteringOptions != FilteringOptions.NewSharedApplications && filteringOptions != FilteringOptions.AllSharedApplications )?
                new BsonDocument
                {
                    {
                        "$match",
                        BuildEmployerReviewMatch()
                    }
                } : new BsonDocument
                {
                    {
                        "$match",
                        BuildSharedApplicationsVacanciesMatch()
                    }
                };

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipelineDocumentCount(match,secondaryMath, string.IsNullOrEmpty(employerAccountId) ? null: employerReviewMatch);

            return await RunAggPipelineCountQuery(aggPipeline);
        }

        private async Task<List<VacancyStatusDashboard>> RunDashboardAggPipelineQuery(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancyDashboard = await RetryPolicy.ExecuteAsync(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancyDashboardAggQueryResponseDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunDashboardAggPipelineQuery)));

            return vacancyDashboard.Select(VacancyDashboardMapper.MapFromVacancyDashboardSummaryResponseDto).ToList();
        }

        private async Task<List<VacancyApplicationsDashboard>> RunApplicationsDashboardAggPipelineQuery(
            BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancyDashboard = await RetryPolicy.ExecuteAsync(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancyApplicationsDashboardResponseDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunApplicationsDashboardAggPipelineQuery)));

            return vacancyDashboard.Select(VacancyDashboardApplicationsMapper.MapFromVacancyApplicationsDashboardResponseDto).ToList();
        }

        private async Task<List<VacancySharedApplicationsDashboard>> RunSharedApplicationsDashboardAggPipelineQuery(
           BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);

            var vacancyDashboard = await RetryPolicy.ExecuteAsync(async context =>
            {
                var aggResults = await collection.AggregateAsync<VacancySharedApplicationsDashboardResponseDto>(pipeline);
                return await aggResults.ToListAsync();
            },
                new Context(nameof(RunSharedApplicationsDashboardAggPipelineQuery)));

            return vacancyDashboard.Select(VacancyDashboardSharedApplicationsMapper.MapFromVacancySharedApplicationsDashboardResponseDto).ToList();
        }

        private async Task<long> RunAggPipelineCountQuery(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancySummaries = await RetryPolicy.ExecuteAsync(async context =>
                {
                    var aggResults = await collection.AggregateAsync<CountResponseDto>(pipeline);
                    return await aggResults.FirstOrDefaultAsync();
                },
                new Context(nameof(RunAggPipelineCountQuery)));
            return vacancySummaries?.Total ?? 0;
        }

        private async Task<IList<VacancySummary>> RunAggPipelineQuery(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancySummaries = await RetryPolicy.ExecuteAsync(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancySummaryAggQueryResponseDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunAggPipelineQuery)));

            return vacancySummaries
                    .Select(VacancySummaryMapper.MapFromVacancySummaryAggQueryResponseDto)
                    .ToList();
        }

        private static BsonDocument BuildSecondaryBsonDocumentFilter(FilteringOptions? filteringOptions, string searchTerm)
        {
            var document = new BsonDocument();
            
            if (filteringOptions.HasValue)
            {
                var bsonArray = new BsonArray();
                switch (filteringOptions)
                {
                    case FilteringOptions.NewApplications:
                        bsonArray.Add(ApplicationReviewStatus.New.ToString());
                        break;
                    case FilteringOptions.AllApplications:
                        bsonArray.Add(ApplicationReviewStatus.New.ToString());
                        bsonArray.Add(ApplicationReviewStatus.Unsuccessful.ToString());
                        bsonArray.Add(ApplicationReviewStatus.Successful.ToString());
                        break;
                    case FilteringOptions.NewSharedApplications:
                        bsonArray.Add(ApplicationReviewStatus.Shared.ToString());
                        break;
                    case FilteringOptions.AllSharedApplications:
                        document.Add("candidateApplicationReview.dateSharedWithEmployer", new BsonDocument { { "$gt", "1900-01-01T01:00:00.389Z" } });
                        break;
                    case FilteringOptions.EmployerReviewedApplications:
                        bsonArray.Add(ApplicationReviewStatus.EmployerUnsuccessful.ToString());
                        bsonArray.Add(ApplicationReviewStatus.EmployerInterviewing.ToString());
                        break;
                    case FilteringOptions.ClosingSoonWithNoApplications:
                        document.Add("candidateApplicationReview",  BsonNull.Value );  
                        break;
                }

                if (bsonArray.Count > 0)
                {
                    document.Add("candidateApplicationReview.status", new BsonDocument { { "$in", bsonArray } });    
                    document.Add("candidateApplicationReview.isWithdrawn", new BsonDocument { { "$in", new BsonArray([false, BsonNull.Value]) } });    
                }
                
            }
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var bsonArray = new BsonArray
                {
                    new BsonDocument{{"title",new BsonRegularExpression(Regex.Escape(searchTerm.ToLower()),"i" )}},
                    new BsonDocument{{"legalEntityName",new BsonRegularExpression(Regex.Escape(searchTerm.ToLower()),"i" )}}
                };
                if (long.TryParse(searchTerm.Replace("vac","", StringComparison.CurrentCultureIgnoreCase), out var vacancyRef))
                {
                    bsonArray.Add(new BsonDocument{{"vacancyReference",vacancyRef}});
                }
                document.AddRange( new BsonDocument
                {
                    {"$or", bsonArray}
                });
            }
            
            return document;
        }

        private static BsonDocument BuildEmployerReviewMatch()
        {
            var document = new BsonDocument
            {
                {"$or", new BsonArray
                {
                    new BsonDocument{{"ownerType","Employer"}},
                    new BsonDocument{ {"$and",new BsonArray{ new BsonDocument{{"ownerType","Provider"}},new BsonDocument{{"status","Review"}}}} }
                }}
            };
            
            return document;
        }

        private static BsonDocument BuildSharedApplicationsVacanciesMatch()
        {
            var document = new BsonDocument
            {
                {"$and", new BsonArray
                {
                    new BsonDocument{{"ownerType","Provider"}},
                    new BsonDocument{{"$or", new BsonArray{ new BsonDocument{{"status", "Live"}}, new BsonDocument{{"status","Closed"}} }} }
                }}
            };

            return document;
        }

        private static BsonDocument BuildBsonDocumentFilterValues(long? ukprn, string employerAccountId, FilteringOptions? status, BsonArray bsonArray)
        {
            var document = new BsonDocument
            {
                { "isDeleted", false },
                { "vacancyType", new BsonDocument{{"$in", bsonArray} } },
            };

            if (ukprn.HasValue)
            {
                document.Add("ownerType", "Provider");
                document.Add("trainingProvider.ukprn", ukprn.Value);
            }
            if (!string.IsNullOrEmpty(employerAccountId))
            {
                document.Add("employerAccountId", employerAccountId);
            }

            if (status.HasValue)
            {
                switch (status)
                {
                    case FilteringOptions.Draft:
                    case FilteringOptions.Live:
                    case FilteringOptions.Closed:
                    case FilteringOptions.Review:
                    case FilteringOptions.Submitted:
                        document.Add("status", status.Value.ToString());
                        break;
                    case FilteringOptions.Referred:
                        var statuses = new BsonArray
                        {
                            VacancyStatus.Referred.ToString(),
                            VacancyStatus.Rejected.ToString()
                        };
                        document.Add("status", new BsonDocument{{"$in", statuses}});
                        break;
                    case FilteringOptions.ClosingSoon:
                        document.Add("status", VacancyStatus.Live.ToString());
                        document.Add("closingDate", new BsonDocument {{"$lte",BsonDateTime.Create(DateTime.UtcNow.AddDays(ClosingSoonDays))}});
                        break;
                    case FilteringOptions.ClosingSoonWithNoApplications:
                        document.Add("status", VacancyStatus.Live.ToString());
                        document.Add("closingDate", new BsonDocument {{"$lte",BsonDateTime.Create(DateTime.UtcNow.AddDays(ClosingSoonDays))}});
                        document.Add("applicationMethod", ApplicationMethod.ThroughFindAnApprenticeship.ToString());
                        break;
                    case FilteringOptions.Transferred:
                        document.Add("transferInfo.transferredDate", new BsonDocument {{"$nin", new BsonArray {BsonNull.Value}}});
                        break;
                    case FilteringOptions.NewSharedApplications:
                    case FilteringOptions.AllSharedApplications:
                        var vacancyStatuses = new BsonArray
                        {
                            VacancyStatus.Live.ToString(),
                            VacancyStatus.Closed.ToString()
                        };
                        document.Add("status", new BsonDocument{{"$in", vacancyStatuses }});
                        document.Add("ownerType", "Provider");
                        break;
                    case FilteringOptions.Dashboard:
                        document.Add("status", new BsonDocument{{"$in", new BsonArray
                        {
                            VacancyStatus.Live.ToString(),
                            VacancyStatus.Closed.ToString()
                        } }});
                        break;
                }
                
            }

            return document;
        }

        private class VacancyTransferInfo
        {
            public TransferInfo TransferInfo { get; set; }
        }
    }
}