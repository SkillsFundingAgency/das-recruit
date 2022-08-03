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

        public VacancySummariesProvider(
            ILoggerFactory loggerFactory, 
            IOptions<MongoDbConnectionDetails> details, 
            IVacancyTaskListStatusService vacancyTaskListStatusService)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
            _vacancyTaskListStatusService = vacancyTaskListStatusService;
        }
        
        public async Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountAsync(string employerAccountId)
        {
            var match = new BsonDocument
            {
                {
                    "$match",
                    new BsonDocument
                    {
                        { "employerAccountId", employerAccountId },
                        { "ownerType", OwnerType.Employer.ToString() },
                        { "isDeleted", false }
                    }
                }
            };

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesInReviewByEmployerAccountAsync(string employerAccountId)
        {
            var match = new BsonDocument
            {
                {
                    "$match",
                    new BsonDocument
                    {
                        { "employerAccountId", employerAccountId },
                        { "ownerType", OwnerType.Provider.ToString() },
                        { "status", VacancyStatus.Review.ToString() },
                        { "isDeleted", false }
                    }
                }
            };

            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<VacancyDashboard>> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, VacancyType vacancyType)
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
                    new BsonDocument
                    {
                        { "trainingProvider.ukprn", ukprn },
                        { "ownerType", OwnerType.Provider.ToString() },
                        { "isDeleted", false },
                        { "vacancyType", new BsonDocument{{"$in", bsonArray} } }
                    }
                }
            };
            var aggPipelines = VacancySummaryAggQueryBuilder.GetAggregateQueryPipelineDashboard(match);
            return await RunDashboardAggPipelineQuery(aggPipelines);
        }
        
        public async Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, VacancyType vacancyType)
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
                    new BsonDocument
                    {
                        { "trainingProvider.ukprn", ukprn },
                        { "ownerType", OwnerType.Provider.ToString() },
                        { "isDeleted", false },
                        { "vacancyType", new BsonDocument{{"$in", bsonArray} } }
                    }
                }
            };
            
            var aggPipeline = VacancySummaryAggQueryBuilder.GetAggregateQueryPipeline(match);

            return await RunAggPipelineQuery(aggPipeline);
        }

        public async Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn, VacancyType vacancyType)
        {
            var builder = Builders<VacancyTransferInfo>.Filter;
            var filter = builder.Eq(TransferInfoUkprn, ukprn) &
                         builder.Eq(TransferInfoReason, TransferReason.EmployerRevokedPermission.ToString()) &
                         builder.AnyEq("VacancyType",new []{"Apprenticeship", null});

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

        public async Task<long> VacancyCount(long ukprn, VacancyType vacancyType)
        {
            var builder = Builders<Vacancy>.Filter;
            
            var filter = builder.Eq("trainingProvider.ukprn", ukprn) &
                         builder.Eq("isDeleted", false) &
                         builder.AnyEq("VacancyType",new []{"Apprenticeship", null});

            if (vacancyType == VacancyType.Traineeship)
            {
                filter = builder.Eq("trainingProvider.ukprn", ukprn) &
                         builder.Eq("isDeleted", false) &
                         builder.Eq("VacancyType", "Traineeship" );
            }
            
            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.Execute(_ =>
                collection.CountDocumentsAsync(filter), new Context(nameof(VacancyCount)));
            return result;
        }

        private async Task<List<VacancyDashboard>> RunDashboardAggPipelineQuery(BsonDocument[] pipeline)
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

        private class VacancyTransferInfo
        {
            public TransferInfo TransferInfo { get; set; }
        }
    }
}