using System;
using System.Collections.Generic;
using System.Linq;
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
       
        public async Task<VacancyDashboard> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, VacancyType vacancyType)
        {
            var bsonArray = new BsonArray
            {
                vacancyType.ToString()
            };
            
            if (vacancyType == VacancyType.Apprenticeship)
            {
                bsonArray.Add(BsonNull.Value);
            }
            
            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, null, null, bsonArray, null)
                }
            };
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(match);
            var closingSoonAggPipeline = builder.GetAggregateQueryPipelineVacanciesClosingSoonDashboard(match);
            var dashboardValuesTask =  RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);
            var closingSoonDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(closingSoonAggPipeline);

            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask, closingSoonDashboardValuesTask);
            
            return new VacancyDashboard
            {
                VacancyStatusDashboard = dashboardValuesTask.Result,
                VacancyApplicationsDashboard = applicationDashboardValuesTask.Result,
                VacanciesClosingSoonWithNoApplications = closingSoonDashboardValuesTask.Result.FirstOrDefault()?.StatusCount ?? 0
            };
        }
        public async Task<VacancyDashboard> GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(string employerAccountId, VacancyType vacancyType)
        {
            var bsonArray = new BsonArray
            {
                vacancyType.ToString()
            };
            
            if (vacancyType == VacancyType.Apprenticeship)
            {
                bsonArray.Add(BsonNull.Value);
            }
            
            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null, employerAccountId, null, bsonArray, null)
                }
            };
            var employerReviewMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildEmployerReviewMatch()
                }
            };
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match,employerReviewMatch);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(match, employerReviewMatch);
            
            var dashboardValuesTask = RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask = RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);

            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask);
            return new VacancyDashboard
            {
                VacancyStatusDashboard = dashboardValuesTask.Result,
                VacancyApplicationsDashboard = applicationDashboardValuesTask.Result
            };
        }
        
        public async Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, VacancyType vacancyType, int page, FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                vacancyType.ToString()
            };
            
            if (vacancyType == VacancyType.Apprenticeship)
            {
                bsonArray.Add(BsonNull.Value);
            }

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn,string.Empty, status, bsonArray, vacancyType)
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

        public async Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountId(string employerAccountId, VacancyType vacancyType, int page,
            FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                vacancyType.ToString()
            };
            
            if (vacancyType == VacancyType.Apprenticeship)
            {
                bsonArray.Add(BsonNull.Value);
            }

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null,employerAccountId, status, bsonArray, vacancyType)
                }
            };
            var employerReviewMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildEmployerReviewMatch()
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

        public async Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn, VacancyType vacancyType)
        {
            var builder = Builders<VacancyTransferInfo>.Filter;
            var filter = builder.Eq(TransferInfoUkprn, ukprn) &
                         builder.Eq(TransferInfoReason, TransferReason.EmployerRevokedPermission.ToString()) &
                         builder.In("VacancyType",new []{"Apprenticeship", null});

            if (vacancyType == VacancyType.Traineeship)
            {
                filter = builder.Eq(TransferInfoUkprn, ukprn) &
                        builder.Eq(TransferInfoReason, TransferReason.EmployerRevokedPermission.ToString()) &
                        builder.Eq("VacancyType", "Traineeship" );
            }
            
            var collection = GetCollection<VacancyTransferInfo>();

            var result = await RetryPolicy.Execute(_ =>
                    collection.Find(filter)
                        .Project<VacancyTransferInfo>(GetProjection<VacancyTransferInfo>())
                        .ToListAsync(),
                new Context(nameof(GetTransferredFromProviderAsync)));

            return result.Select(v => v.TransferInfo).ToList();
        }

        public async Task<long> VacancyCount(long? ukprn, string employerAccountId, VacancyType vacancyType, FilteringOptions? filteringOptions, string searchTerm, OwnerType ownerType)
        {
            var bsonArray = new BsonArray
            {
                vacancyType.ToString()
            };
            
            if (vacancyType == VacancyType.Apprenticeship)
            {
                bsonArray.Add(BsonNull.Value);
            }

            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, employerAccountId, filteringOptions, bsonArray, vacancyType)
                }
            };
            var secondaryMath = new BsonDocument
            {
                {
                    "$match",
                    BuildSecondaryBsonDocumentFilter(filteringOptions, searchTerm)
                }
            };

            var employerReviewMatch = ownerType == OwnerType.Employer ? 
                new BsonDocument
                {
                    {
                        "$match",
                        BuildEmployerReviewMatch()
                    }
                } : null;
            
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipelineDocumentCount(match,secondaryMath, employerReviewMatch);

            return await RunAggPipelineCountQuery(aggPipeline);
        }

        private async Task<List<VacancyStatusDashboard>> RunDashboardAggPipelineQuery(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancyDashboard = await RetryPolicy.Execute(async context =>
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
            
            var vacancyDashboard = await RetryPolicy.Execute(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancyApplicationsDashboardResponseDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunApplicationsDashboardAggPipelineQuery)));

            return vacancyDashboard.Select(VacancyDashboardApplicationsMapper.MapFromVacancyApplicationsDashboardResponseDto).ToList();
        }

        private async Task<long> RunAggPipelineCountQuery(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancySummaries = await RetryPolicy.Execute(async context =>
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
            
            var vacancySummaries = await RetryPolicy.Execute(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancySummaryAggQueryResponseDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunAggPipelineQuery)));

            return vacancySummaries
                    .Select(dto => VacancySummaryMapper.MapFromVacancySummaryAggQueryResponseDto(dto, _vacancyTaskListStatusService.IsTaskListCompleted(dto.Id)))
                    .ToList();
        }

        private static BsonDocument BuildSecondaryBsonDocumentFilter(FilteringOptions? filteringOptions, string searchTerm)
        {
            var document = new BsonDocument();

            if (filteringOptions.HasValue)
            {
                switch (filteringOptions)
                {
                    case FilteringOptions.NewApplications:
                        document.Add("noOfNewApplications", new BsonDocument {{"$gt", 0}});
                        break;
                    case FilteringOptions.AllApplications:
                        document.Add("noOfApplications", new BsonDocument {{"$gt", 0}});
                        break;
                    case FilteringOptions.ClosingSoonWithNoApplications:
                        document.Add("noOfApplications", 0);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                document.Add("_id.searchField", new BsonRegularExpression($"/{searchTerm.ToLower()}/"));
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
        
        private static BsonDocument BuildBsonDocumentFilterValues(long? ukprn, string employerAccountId, FilteringOptions? status, BsonArray bsonArray, VacancyType? vacancyType)
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
                        document.Add("applicationMethod", vacancyType == VacancyType.Apprenticeship ? ApplicationMethod.ThroughFindAnApprenticeship:ApplicationMethod.ThroughFindATraineeship);
                        break;
                    case FilteringOptions.Transferred:
                        document.Add("transferInfo.transferredDate", new BsonDocument {{"$nin", new BsonArray {BsonNull.Value}}});
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