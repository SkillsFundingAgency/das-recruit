using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    internal class ProviderApplicationsReportStrategy : MongoDbCollectionBase, IReportStrategy
    {
        private const string QueryUkprn = "_ukprn_";
        private const string QueryFromDate = "_fromDate_";
        private const string QueryToDate = "_toDate_";

        private const string ColumnProgramme = "Programme";
        private const string ColumnProgrammeStatus = "Programme_Status";
        private const string ColumnApplicationLastUpdatedDate = "Application_LastUpdatedDate";
        private const string ColumnApplicationDate = "Application_Date";
        private const string ColumnNumberOfDaysAppAtThisStatus = "Number_Of_Days_App_At_This_Status";

        private readonly IApprenticeshipProgrammeProvider _programmeProvider;
        private readonly ITimeProvider _timeProvider;

        private const string QueryFormat = @"[
            { $match: {'ownerType' : 'Provider', 'trainingProvider.ukprn' : _ukprn_}},
            { $lookup: { from: 'applicationReviews', localField: 'vacancyReference', foreignField: 'vacancyReference', as: 'ar'}},
            { $unwind: '$ar'},
            { $match: {'ar.submittedDate' : { $gte: ISODate('_fromDate_'), $lt: ISODate('_toDate_')}}},
            { $project: {
                    '_id' : 0,
                    'Candidate_Name' : { $concat: ['$ar.application.firstName', ' ', '$ar.application.lastName']},
                    'Applicant_id' : '$ar._id',
                    'Address_Line1' : { $ifNull: ['$ar.application.addressLine1', null]},
                    'Address_Line2' : { $ifNull: ['$ar.application.addressLine2', null]},
                    'Address_Line3' : { $ifNull: ['$ar.application.addressLine3', null]},
                    'Address_Line4' : { $ifNull: ['$ar.application.addressLine4', null]},
                    'Postcode' : { $ifNull: ['$ar.application.postcode', null]},
                    'Telephone' : { $ifNull: ['$ar.application.phone', null]},
                    'School' : { $ifNull: ['$ar.application.educationInstitution', null]},
                    'Date_of_Birth' : { $ifNull: ['$ar.application.birthDate', null]},
                    'Vacancy_Reference_Number' : '$vacancyReference',
                    'Vacancy_Title' : '$title',
                    'Programme' : { $ifNull: ['$programmeId', null]},
                    'Programme_Status' : null,
                    'Employer' : { $ifNull: ['$employerName', null]},
                    'Vacancy_Postcode' : { $ifNull: ['$employerLocation.postcode', null]},
                    'Learning_Provider' : { $ifNull: ['$trainingProvider.name', null]},
                    'Application_Date' : { $ifNull: ['$ar.submittedDate', null]},
                    'Vacancy_Closing_Date' : { $ifNull: ['$closingDate', null]},
                    'Application_Status' : { $ifNull: ['$ar.status', null]},
                    'Application_LastUpdatedDate' : { $ifNull: ['$ar.statusUpdatedDate', null]}
                }
            }]";

        public ProviderApplicationsReportStrategy(
            ILoggerFactory loggerFactory, 
            IOptions<MongoDbConnectionDetails> details,
            IApprenticeshipProgrammeProvider programmeProvider,
            ITimeProvider timeProvider
            ) : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
            _programmeProvider = programmeProvider;
            _timeProvider = timeProvider;
        }

        public Task<string> GetReportDataAsync(Dictionary<string,object> parameters)
        {
            var ukprn = long.Parse(parameters[ReportParameterName.Ukprn].ToString());
            var fromDate = (DateTime) parameters[ReportParameterName.FromDate];
            var toDate = (DateTime)parameters[ReportParameterName.ToDate];

            return GetProviderApplicationsAsync(ukprn, fromDate, toDate);
        }

        private async Task<string> GetProviderApplicationsAsync(long ukprn, DateTime fromDate, DateTime toDate)
        {
            var collection = GetCollection<BsonDocument>();

            var queryJson = QueryFormat
                .Replace(QueryUkprn, ukprn.ToString())
                .Replace(QueryFromDate, fromDate.ToString("o"))
                .Replace(QueryToDate , toDate.ToString("o"));

            var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument[]>(queryJson);

            var results = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Aggregate<BsonDocument>(bson).ToListAsync(),
            new Context(nameof(GetProviderApplicationsAsync)));

            await ProcessResultsAsync(results);

            var dotNetFriendlyResults = results.Select(BsonTypeMapper.MapToDotNetValue);
            var jsonArray = JsonConvert.SerializeObject(dotNetFriendlyResults);
            
            return jsonArray;
        }

        private async Task ProcessResultsAsync(List<BsonDocument> results)
        {
            foreach (var result in results)
            {
                await SetProgramme(result);
                SetNumberOfDaysAtThisStatus(result);
            }
        }

        private async Task SetProgramme(BsonDocument result)
        {    
            var programmeId = result[ColumnProgramme].AsString;
            var programme = await _programmeProvider.GetApprenticeshipProgrammeAsync(programmeId);

            var programmeValue = $"{programme.Id} {programme.Title} ({programme.ApprenticeshipType.ToString()})";
            result[ColumnProgramme] = programmeValue;
            result[ColumnProgrammeStatus] = programme.IsActive ? "Active" : "Inactive";
        }

        private void SetNumberOfDaysAtThisStatus(BsonDocument result)
        {
            var statusDate = result[ColumnApplicationLastUpdatedDate].ToNullableUniversalTime() ?? 
                             result[ColumnApplicationDate].ToUniversalTime();

            var statusTimespan = _timeProvider.Now.Subtract(statusDate);

            result.InsertAt(result.IndexOfName(ColumnApplicationLastUpdatedDate), 
                new BsonElement(ColumnNumberOfDaysAppAtThisStatus, statusTimespan.Days));
            result.Remove(ColumnApplicationLastUpdatedDate);
        }
    }
}
