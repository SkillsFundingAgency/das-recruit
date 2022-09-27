using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    internal class QaApplicationsReportStrategy : MongoDbCollectionBase, IReportStrategy
    {
        private const string QueryFromDate = "_fromDate_";
        private const string QueryToDate = "_toDate_";

        private class Column
        {
            public const string DateSubmitted = "Date submitted";
            public const string DisplayName = "Display name";
            public const string Employer = "Employer";
            public const string FrameworkStatus = "frameworkStatus";
            public const string NumberOfIssuesReported = "Number of issues reported";
            public const string Outcome = "Outcome";
            public const string ProgrammeId = "programmeId";
            public const string ReviewCompleted = "Review completed";
            public const string ReviewedBy = "Reviewed by";
            public const string ReviewStarted = "Review started";
            public const string SlaDeadline = "SLA deadline";
            public const string SlaExceededByHours = "SLA exceeded by (hours)";
            public const string StandardOrFramework = "Standard / Framework";
            public const string SubmissionNumber = "Submission number";
            public const string TimeTakenToReview = "Time taken to review";
            public const string TrainingProvider = "Training provider";
            public const string VacancyPostcode = "Vacancy postcode";
            public const string VacancyReferenceNumber = "Vacancy reference number";
            public const string VacancySubmittedBy = "Vacancy submitted by";
            public const string VacancyTitle = "Vacancy title";
        }


        private readonly IApprenticeshipProgrammeProvider _programmeProvider;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger<QaApplicationsReportStrategy> _logger;
        private readonly IMongoCollection<BsonDocument> _collection;

        private const string QueryFormat = @"[
            { $match: { createdDate: { $gt: ISODate('_fromDate_'), $lte: ISODate('_toDate_')}} },
            { $project: {
                _id: 0
                ,'Vacancy title': { $ifNull: ['$title', null] }
                ,'Vacancy reference number': { $ifNull: ['$vacancyReference', null] }
                ,'Submission number' : { $ifNull: ['$submissionCount', null] }
                ,'Date submitted': { $ifNull: ['$createdDate', null] }
                ,'SLA deadline': { $ifNull: ['$slaDeadline', null] }
                ,'Review started': { $ifNull: ['$reviewedDate', null] }
                ,'Review completed': { $ifNull: ['$closedDate', null] }
                ,'Outcome': {$ifNull : ['$manualOutcome', null] }
                ,'Number of issues reported': {
                    $size: {
                        $filter: {
                            input: {$ifNull: ['$manualQaFieldIndicators', []] },
                            as: 'field',
                            cond: { $eq: ['$$field.isChangeRequested', true] }
                        }
                    }
                }
                ,'Vacancy submitted by': { $ifNull: ['$vacancySnapshot.ownerType', null] }
                ,'Vacancy submitted by user': { $ifNull: ['$submittedByUser.email', null] }
                ,'Employer': { $ifNull: ['$vacancySnapshot.legalEntityName', null] }
                ,'Display name': { $ifNull: ['$vacancySnapshot.employerName', null] }
                ,'Training provider': { $ifNull: ['$vacancySnapshot.trainingProvider.name', null] }
                ,'Vacancy postcode': { $ifNull: ['$vacancySnapshot.employerLocation.postcode', null] }
                ,'programmeId': { $ifNull: ['$vacancySnapshot.programmeId', null] }
                ,'Referred Fields' : { 
                    $filter: {
                        input: {$ifNull: ['$manualQaFieldIndicators', []] },
                        as: 'field',
                        cond: { $eq: ['$$field.isChangeRequested', true] }
                    } 
                }
                ,'Reviewed by': { $ifNull: ['$reviewedByUser.userId', null] }
                ,'Reviewer Comment': { $ifNull: ['$manualQaComment', null] }
            }}
        ]";

        public QaApplicationsReportStrategy(
            ILoggerFactory loggerFactory,
            IOptions<MongoDbConnectionDetails> details,
            IApprenticeshipProgrammeProvider programmeProvider,
            ITimeProvider timeProvider,
            ILogger<QaApplicationsReportStrategy> logger)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.VacancyReviews, details)
        {
            _programmeProvider = programmeProvider;
            _timeProvider = timeProvider;
            _logger = logger;
            _collection = GetCollection<BsonDocument>();
            RetryPolicy = MongoDbRetryPolicy.GetConnectionRetryPolicy(_logger);
        }

        public ReportDataType ResolveFormat(string fieldName)
        {
            if (fieldName.Equals("Referred Fields", StringComparison.CurrentCultureIgnoreCase))
            {
                return ReportDataType.ArrayType;
            }
            return ReportDataType.StringType;
        }

        public Task<ReportStrategyResult> GetReportDataAsync(Dictionary<string, object> parameters)
        {
            var fromDate = (DateTime)parameters[ReportParameterName.FromDate];
            var toDate = (DateTime)parameters[ReportParameterName.ToDate];

            return GetApplicationReviewsAsync(fromDate, toDate);
        }

        private Task<ReportStrategyResult> GetApplicationReviewsAsync(DateTime fromDate, DateTime toDate)
        {
            var results = new List<BsonDocument>();
            _logger.LogInformation($"Report parameters fromDate:{fromDate} toDate:{toDate} returned {results.Count} results");

            var queryJson = QueryFormat
                .Replace(QueryFromDate, fromDate.AddTicks(-1).ToString("o"))
                .Replace(QueryToDate, toDate.ToString("o"));
            
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Date", _timeProvider.Now.ToUkTime().ToString("dd/MM/yyyy HH:mm:ss"))
            };

            return Task.FromResult(new ReportStrategyResult(headers, "", queryJson));
        }

        public async Task<string> GetApplicationReviewsRecursiveAsync(string queryJson)
        {
            var pageNumber = 1;
            var results = new List<BsonDocument>();
            var currentResults = await GetCurrentPageOfResults(pageNumber, queryJson);
            results.AddRange(currentResults);
            
            while (currentResults.Any())
            {
                pageNumber++;

                currentResults = await GetCurrentPageOfResults(pageNumber, queryJson);
                
                results.AddRange(currentResults);    
            }
            var dotNetFriendlyResults = results
                .OrderBy(x => x[Column.DateSubmitted].ToUniversalTime())
                .ThenBy(x => x[Column.VacancyReferenceNumber].ToInt64())
                .Select(BsonTypeMapper.MapToDotNetValue);
            
            return JsonConvert.SerializeObject(dotNetFriendlyResults);
        }

        private async Task<List<BsonDocument>> GetCurrentPageOfResults(int pageNumber, string queryJson)
        {
            var queryBson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(queryJson);
            queryBson.Insert(queryBson.Count, new BsonDocument {{"$skip", (pageNumber-1) * 500}});
            queryBson.Insert(queryBson.Count, new BsonDocument {{"$limit",500}});
            var pipelineDefinition = queryBson.Values.Select(p => p.ToBsonDocument()).ToArray();
            List<BsonDocument> currentResults =
                await RetryPolicy.Execute(_ =>
                        _collection.Aggregate<BsonDocument>(pipelineDefinition).ToListAsync(),
                    new Context(nameof(GetApplicationReviewsAsync)));
            await ProcessResultsAsync(currentResults);

            return currentResults;
        }

        private async Task ProcessResultsAsync(List<BsonDocument> results)
        {
            foreach (var result in results)
            {
                await SetProgrammeAsync(result);
                SetDurations(result);
            }
        }

        private async Task SetProgrammeAsync(BsonDocument result)
        {
            if (result[Column.ProgrammeId].IsBsonNull)
            {
                result.InsertAt(result.IndexOfName(Column.ProgrammeId),
                    new BsonElement(Column.StandardOrFramework, (BsonValue)"unknown"));
                result.Remove(Column.ProgrammeId);
                return;
            }
            
            var programmeId = result[Column.ProgrammeId].AsString;
            
            var programme = await _programmeProvider.GetApprenticeshipProgrammeAsync(programmeId);

            var programmeValue = programme != null ? $"{programme.Id} {programme.Title}" : programmeId;
            
            result.InsertAt(result.IndexOfName(Column.ProgrammeId),
                new BsonElement(Column.StandardOrFramework, (BsonValue)programmeValue));
            
            result.InsertAt(result.IndexOfName(Column.StandardOrFramework) + 1,
                new BsonElement("level", (BsonValue)(programme != null ? programme.ApprenticeshipLevel.ToString() : "")));
            
            
            result.Remove(Column.ProgrammeId);
        }

        private void SetDurations(BsonDocument result)
        {
            DateTime slaDeadline = result[Column.SlaDeadline].ToLocalTime();
            DateTime? reviewedDate =
                result.IndexOfName(Column.ReviewStarted) >= 0
                ? result[Column.ReviewStarted].ToNullableLocalTime()
                : null;
            DateTime? closedDate =
                result.IndexOfName(Column.ReviewCompleted) >= 0
                ? result[Column.ReviewCompleted].ToNullableLocalTime()
                : null;
            DateTime effectiveClosedDate = closedDate ?? _timeProvider.Now;

            int indexOfManualOutcomeColumn = result.IndexOfName(Column.Outcome);

            string slaExceededByHours =
                effectiveClosedDate > slaDeadline
                ? (effectiveClosedDate - slaDeadline).TotalHours.ToString("f2")
                : "";
            result.InsertAt(indexOfManualOutcomeColumn,
                new BsonElement(Column.SlaExceededByHours, slaExceededByHours));

            string reviewTime = "";
            if (reviewedDate.HasValue)
            {
                TimeSpan duration = effectiveClosedDate - reviewedDate.Value;
                string totalHours = Math.Floor(duration.TotalHours).ToString();
                reviewTime = totalHours + ":" + duration.ToString("mm':'ss");
            }
            result.InsertAt(indexOfManualOutcomeColumn,
                new BsonElement(Column.TimeTakenToReview, reviewTime));
        }
    }
}
