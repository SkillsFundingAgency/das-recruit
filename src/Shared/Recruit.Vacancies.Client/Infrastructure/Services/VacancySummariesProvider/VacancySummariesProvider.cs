using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal sealed class VacancySummariesProvider(
        ILoggerFactory loggerFactory,
        IOptions<MongoDbConnectionDetails> details,
        IFeature feature)
        : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details),
            IVacancySummariesProvider
    {
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private const string TransferInfoUkprn = "transferInfo.ukprn";
        private const string TransferInfoReason = "transferInfo.reason";
        private const int ClosingSoonDays = 5;
        private readonly bool _isMongoMigrationFeatureEnabled = false;

        public VacancySummariesProvider(
            ILoggerFactory loggerFactory,
            IOptions<MongoDbConnectionDetails> details,
            IFeature feature,
            ITrainingProviderService trainingProviderService,
            IEmployerAccountProvider employerAccountProvider)
            : this(loggerFactory, details, feature)
        {
            _trainingProviderService = trainingProviderService;
            _employerAccountProvider = employerAccountProvider;
            _isMongoMigrationFeatureEnabled = feature.IsFeatureEnabled(FeatureNames.MongoMigration);
        }
       
        public async Task<VacancyDashboard> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, bool ignoreGetApplicationReview)
        {
            var bsonArray = new BsonArray
            {
                "Apprenticeship",
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
            var closingSoonMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn, null, FilteringOptions.ClosingSoonWithNoApplications, bsonArray)
                }
            };
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(applicationsMatch);
            var closingSoonAggPipeline = builder.GetAggregateQueryPipelineVacanciesClosingSoonDashboard(closingSoonMatch, ignoreGetApplicationReview);
            var dashboardValuesTask =  RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask =  ignoreGetApplicationReview ? Task.FromResult(new List<VacancyApplicationsDashboard>()) : RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);
            var closingSoonDashboardValuesTask =ignoreGetApplicationReview ? Task.FromResult(new List<VacancyApplicationsDashboard>()) : RunApplicationsDashboardAggPipelineQuery(closingSoonAggPipeline);
            var closingSoonReferencesTask = RunClosingSoonVacancies(closingSoonAggPipeline);

            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask, closingSoonDashboardValuesTask,closingSoonReferencesTask);

            // If the Mongo migration feature is enabled, retrieve the application review stats
            
            var closingSoonReferences = await closingSoonReferencesTask;
            if (closingSoonReferences == null || closingSoonReferences.Count == 0)
            {
                return new VacancyDashboard
                {
                    VacancyStatusDashboard = await dashboardValuesTask,
                    VacancyApplicationsDashboard = ignoreGetApplicationReview ? [] : await applicationDashboardValuesTask,
                    VacanciesClosingSoonWithNoApplications = 0
                };
            }

            var vacancyReferences = closingSoonReferences
                .Distinct()
                .ToList();

            // Retrieve application review stats for vacancies closing soon
            var dashboardStats = await _trainingProviderService.GetProviderDashboardApplicationReviewStats(
                ukprn,
                vacancyReferences
            );

            // Create a lookup with safe handling for duplicate keys
            var applicationReviewStatsLookup = dashboardStats.ApplicationReviewStatsList
                .GroupBy(x => x.VacancyReference)
                .ToDictionary(g => g.Key, g => g.First());

            // Count vacancies with no applications
            int closingSoonCount = closingSoonReferences.Count(vacancySummary =>
                applicationReviewStatsLookup.TryGetValue(vacancySummary, out var stats) &&
                stats.HasNoApplications
            );
            
            var closingSoonDashboardValues = await closingSoonDashboardValuesTask;
            // Return the final dashboard view model
            return new VacancyDashboard
            {
                VacancyStatusDashboard = await dashboardValuesTask,
                VacancyApplicationsDashboard = await applicationDashboardValuesTask,
                VacanciesClosingSoonWithNoApplications = ignoreGetApplicationReview
                    ? closingSoonCount
                    : closingSoonDashboardValues.FirstOrDefault(c => c.ClosingSoon)?.StatusCount ?? 0
            };
        }
        public async Task<VacancyDashboard> GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(string employerAccountId, bool ignoreGetApplicationReview)
        {
            var bsonArray = new BsonArray
            {
                "Apprenticeship",
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
            var closingSoonMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(null, employerAccountId, FilteringOptions.ClosingSoonWithNoApplications, bsonArray)
                }
            };
            var builder = new VacancySummaryAggQueryBuilder();
            var aggPipelines = builder.GetAggregateQueryPipelineDashboard(match,employerReviewMatch);
            var applicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(applicationsMatch, employerReviewMatch);
            var sharedApplicationAggPipeline = builder.GetAggregateQueryPipelineDashboardApplications(match, liveVacanciesMatch);
            var closingSoonAggPipeline =builder.GetAggregateQueryPipelineVacanciesClosingSoonDashboard(closingSoonMatch, ignoreGetApplicationReview, employerReviewMatch);
            
            var dashboardValuesTask = RunDashboardAggPipelineQuery(aggPipelines);
            var applicationDashboardValuesTask = ignoreGetApplicationReview ? Task.FromResult(new List<VacancyApplicationsDashboard>()) : RunApplicationsDashboardAggPipelineQuery(applicationAggPipeline);
            var sharedApplicationDashboardValuesTask = ignoreGetApplicationReview ? Task.FromResult(new List<VacancySharedApplicationsDashboard>()) : RunSharedApplicationsDashboardAggPipelineQuery(sharedApplicationAggPipeline);
            var closingSoonDashboardValuesTask =ignoreGetApplicationReview ? Task.FromResult(new List<VacancyApplicationsDashboard>()) :  RunApplicationsDashboardAggPipelineQuery(closingSoonAggPipeline);
            var closingSoonReferencesTask = RunClosingSoonVacancies(closingSoonAggPipeline);
            
            await Task.WhenAll(dashboardValuesTask, applicationDashboardValuesTask, sharedApplicationDashboardValuesTask, closingSoonDashboardValuesTask,closingSoonReferencesTask);

            // If the Mongo migration feature is enabled, retrieve the application review stats
            var closingSoonDashboardValues = closingSoonReferencesTask.Result;
            if (closingSoonDashboardValues == null || closingSoonDashboardValues.Count == 0)
            {
                return new VacancyDashboard
                {
                    VacancyStatusDashboard = dashboardValuesTask.Result,
                    VacancySharedApplicationsDashboard = ignoreGetApplicationReview ? [] : sharedApplicationDashboardValuesTask.Result,
                    VacancyApplicationsDashboard = ignoreGetApplicationReview ? [] : applicationDashboardValuesTask.Result,
                    VacanciesClosingSoonWithNoApplications = 0
                };
            }

            var vacancyReferences = closingSoonDashboardValues
                .Distinct()
                .ToList();

            var dashboardStats = await _employerAccountProvider.GetEmployerDashboardApplicationReviewStats(employerAccountId, vacancyReferences, "");

            var applicationReviewStatsLookup = dashboardStats
                .ApplicationReviewStatsList
                .GroupBy(x => x.VacancyReference)
                .ToDictionary(g => g.Key, g => g.First());

            int closingSoonCount = closingSoonReferencesTask.Result.Count(vacancySummary =>
                applicationReviewStatsLookup.TryGetValue(vacancySummary, out var applicationReview) &&
                applicationReview.HasNoApplications);

            return new VacancyDashboard
            {
                VacancyStatusDashboard = dashboardValuesTask.Result,
                VacancyApplicationsDashboard =ignoreGetApplicationReview ? [] : applicationDashboardValuesTask.Result,
                VacancySharedApplicationsDashboard = ignoreGetApplicationReview ? [] : sharedApplicationDashboardValuesTask.Result,
                VacanciesClosingSoonWithNoApplications = ignoreGetApplicationReview
                    ? closingSoonCount
                    : closingSoonDashboardValuesTask.Result.FirstOrDefault(c => c.ClosingSoon)?.StatusCount ?? 0
            };
        }
        
        public async Task<(IList<VacancySummary>, int? totalCount)> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, int page, FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                "Apprenticeship",
                BsonNull.Value
            };
            
            var match = new BsonDocument
            {
                {
                    "$match",
                    BuildBsonDocumentFilterValues(ukprn,string.Empty, status, bsonArray)
                }
            };
            
            
            GetVacanciesDashboardResponse results = null;
            BsonDocument vacancyReferenceMatch = null; 
            int? totalCount = null;
            if (status is FilteringOptions.EmployerReviewedApplications or FilteringOptions.AllApplications or FilteringOptions.NewApplications)
            {
                BsonDocument refs = new BsonDocument();
                var applicationReviewStatusList = new List<ApplicationReviewStatus>();

                if (status == FilteringOptions.EmployerReviewedApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.EmployerInterviewing);
                    applicationReviewStatusList.Add(ApplicationReviewStatus.EmployerUnsuccessful);
                }

                if (status == FilteringOptions.AllApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.New);
                    applicationReviewStatusList.Add(ApplicationReviewStatus.Unsuccessful);
                    applicationReviewStatusList.Add(ApplicationReviewStatus.Successful);
                }
                if (status == FilteringOptions.NewApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.New);
                }
                
                results = await _trainingProviderService.GetProviderDashboardVacanciesByApplicationReviewStatuses(ukprn,
                applicationReviewStatusList, page);
                
                refs.Add("vacancyReference", new BsonDocument { { "$in", new BsonArray(results.Items.Select(result => result.VacancyReference).ToArray()) } });
                vacancyReferenceMatch = new BsonDocument{
                {
                    "$match",
                    refs
                }};
                totalCount = results.TotalCount;
                page = 1;//override 
            }
            
            var secondaryMath = new BsonDocument
            {
                {
                    "$match",
                    BuildSecondaryBsonDocumentFilter(status)
                }
            };   
                
            var searchMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildSearchMatch(searchTerm)
                }
            };
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match, page,secondaryMath, _isMongoMigrationFeatureEnabled,null,vacancyReferenceMatch, searchMatch);

            var pipelineResult = await RunAggPipelineQuery(aggPipeline);

            if (!_isMongoMigrationFeatureEnabled)
                return (pipelineResult,totalCount);

            // If the Mongo migration feature is enabled, retrieve the application review stats
            var vacancyReferences = pipelineResult
                .Where(x => x.VacancyReference.HasValue)
                .Select(x => x.VacancyReference.Value)
                .ToList();

            var dashboardStats = await _trainingProviderService.GetProviderDashboardApplicationReviewStats(ukprn, vacancyReferences);

            var applicationReviewStatsLookup = dashboardStats.ApplicationReviewStatsList
                .ToDictionary(x => x.VacancyReference);

            foreach (var vacancySummary in pipelineResult)
            {
                if (!vacancySummary.VacancyReference.HasValue ||
                    !applicationReviewStatsLookup.TryGetValue(vacancySummary.VacancyReference.Value,
                        out var applicationReview)) continue;

                vacancySummary.NoOfSuccessfulApplications = applicationReview.SuccessfulApplications;
                vacancySummary.NoOfUnsuccessfulApplications = applicationReview.UnsuccessfulApplications;
                vacancySummary.NoOfNewApplications = applicationReview.NewApplications;
                vacancySummary.NoOfSharedApplications = applicationReview.SharedApplications;
                vacancySummary.NoOfAllSharedApplications = applicationReview.AllSharedApplications;
                vacancySummary.NoOfEmployerReviewedApplications = applicationReview.EmployerReviewedApplications;
            }

            return (pipelineResult,totalCount);
        }

        public async Task<(IList<VacancySummary>, int? totalCount)> GetEmployerOwnedVacancySummariesByEmployerAccountId(string employerAccountId, int page,
            FilteringOptions? status, string searchTerm)
        {
            var bsonArray = new BsonArray
            {
                "Apprenticeship",
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
                    BuildSecondaryBsonDocumentFilter(status)
                }
            };
            
            var searchMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildSearchMatch(searchTerm)
                }
            };
            GetVacanciesDashboardResponse results = null;
            BsonDocument vacancyReferenceMatch = null; 
            int? totalCount = null;
            if (status is FilteringOptions.NewSharedApplications or FilteringOptions.AllSharedApplications or FilteringOptions.AllApplications or FilteringOptions.NewApplications)
            {
                BsonDocument refs = new BsonDocument();
                var applicationReviewStatusList = new List<ApplicationReviewStatus>();

                if (status is FilteringOptions.NewSharedApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.Shared);
                }

                if (status is FilteringOptions.AllSharedApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.AllShared);
                }

                if (status == FilteringOptions.AllApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.New);
                    applicationReviewStatusList.Add(ApplicationReviewStatus.Unsuccessful);
                    applicationReviewStatusList.Add(ApplicationReviewStatus.Successful);
                }
                if (status == FilteringOptions.NewApplications)
                {
                    applicationReviewStatusList.Add(ApplicationReviewStatus.New);
                }
                
                results = await _employerAccountProvider.GetEmployerVacancyDashboardStats(employerAccountId,page,
                    applicationReviewStatusList);
                
                refs.Add("vacancyReference", new BsonDocument { { "$in", new BsonArray(results.Items.Select(result => result.VacancyReference).ToArray()) } });
                vacancyReferenceMatch = new BsonDocument{
                {
                    "$match",
                    refs
                }};
                totalCount = results.TotalCount;
                page = 1;//override 
            }
            
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match, page,secondaryMath,_isMongoMigrationFeatureEnabled, employerReviewMatch,vacancyReferenceMatch, searchMatch);

            var pipelineResult = await RunAggPipelineQuery(aggPipeline);

            if (!_isMongoMigrationFeatureEnabled)
                return (pipelineResult,null);

            // If the Mongo migration feature is enabled, retrieve the application review stats
            var vacancyReferences = pipelineResult
                .Where(x => x.VacancyReference.HasValue)
                .Select(x => x.VacancyReference.Value)
                .ToList();

            var applicationSharedFilteringStatus = "";
            if (status is FilteringOptions.NewSharedApplications)
            {
                applicationSharedFilteringStatus = ApplicationReviewStatus.Shared.ToString();
            }

            if (status is FilteringOptions.AllSharedApplications)
            {
                applicationSharedFilteringStatus = ApplicationReviewStatus.AllShared.ToString();
            }
            
            
            var dashboardStats = await _employerAccountProvider.GetEmployerDashboardApplicationReviewStats(employerAccountId, vacancyReferences, applicationSharedFilteringStatus);

            var applicationReviewStatsLookup = dashboardStats.ApplicationReviewStatsList
                .ToDictionary(x => x.VacancyReference);

            foreach (var vacancySummary in pipelineResult)
            {
                if (!vacancySummary.VacancyReference.HasValue ||
                    !applicationReviewStatsLookup.TryGetValue(vacancySummary.VacancyReference.Value,
                        out var applicationReview)) continue;

                vacancySummary.NoOfSuccessfulApplications = applicationReview.SuccessfulApplications;
                vacancySummary.NoOfUnsuccessfulApplications = applicationReview.UnsuccessfulApplications;
                vacancySummary.NoOfNewApplications = applicationReview.NewApplications;
                vacancySummary.NoOfAllSharedApplications = status == FilteringOptions.AllSharedApplications ? applicationReview.Applications : applicationReview.SharedApplications;
                vacancySummary.NoOfSharedApplications = applicationReview.SharedApplications;
                vacancySummary.NoOfEmployerReviewedApplications = applicationReview.EmployerReviewedApplications;
            }

            return (pipelineResult,totalCount);
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
                "Apprenticeship",
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
                    BuildSecondaryBsonDocumentFilter(filteringOptions)
                }
            };
            var searchMatch = new BsonDocument
            {
                {
                    "$match",
                    BuildSearchMatch(searchTerm)
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

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipelineDocumentCount(match,secondaryMath, string.IsNullOrEmpty(employerAccountId) ? null: employerReviewMatch,  searchMatch, _isMongoMigrationFeatureEnabled);


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

        private async Task<List<long>> RunClosingSoonVacancies(BsonDocument[] pipeline)
        {
            var db = GetDatabase();
            var collection = db.GetCollection<BsonDocument>(MongoDbCollectionNames.Vacancies);
            
            var vacancyDashboard = await RetryPolicy.ExecuteAsync(async context =>
                {
                    var aggResults = await collection.AggregateAsync<VacancyClosingSoonDashboardDto>(pipeline);
                    return await aggResults.ToListAsync();
                },
                new Context(nameof(RunApplicationsDashboardAggPipelineQuery)));

            return vacancyDashboard.Select(c=>c.Id.VacancyReference).ToList();
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

            if (_isMongoMigrationFeatureEnabled)
            {
                var collectionList = pipeline.ToList();
                collectionList.RemoveAll(stage => stage.GetElement(0).Name == "$lookup"
                                                  && stage["$lookup"]["from"] == "applicationReviews");
                pipeline = collectionList.ToArray();
            }
            
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

        private static BsonDocument BuildSecondaryBsonDocumentFilter(FilteringOptions? filteringOptions)
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
            
            return document;
        }

        private static BsonDocument BuildSearchMatch(string searchTerm)
        {
            var document = new BsonDocument();
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

            var vacancyStatuses = new BsonArray
            {
                VacancyStatus.Live.ToString(),
                VacancyStatus.Closed.ToString()
            };
            
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
                        document.Add("status", new BsonDocument{{"$in", vacancyStatuses }});
                        document.Add("ownerType", "Provider");
                        break;
                    case FilteringOptions.NewApplications:    
                    case FilteringOptions.AllApplications:
                        document.Add("status", new BsonDocument{{"$in", vacancyStatuses }});
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