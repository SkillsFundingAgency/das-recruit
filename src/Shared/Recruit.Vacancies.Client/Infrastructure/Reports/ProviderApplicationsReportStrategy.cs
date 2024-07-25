﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
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
        private const string VacancyType = "_vacancytype_";
        private const string ApplicationMethod = "_applicationMethod_";

        private const string ColumnProgramme = "Programme";
        private const string ColumnRoute = "RouteId";
        private const string ColumnApplicationLastUpdatedDate = "Application_LastUpdatedDate";
        private const string ColumnApplicationDate = "Application_Date";
        private const string ColumnNumberOfDaysAppAtThisStatus = "Number_Of_Days_App_At_This_Status";
        private const string ColumnVacancyReference = "Vacancy_Reference_Number";
        private const string ColumnFramework = "Framework";
        private const string ColumnFrameworkStatus = "Framework_Status";
        private const string ColumnStandard = "Standard";
        private const string ColumnRouteName = "Route";
        private const string ColumnStandardStatus = "Standard_Status";
        private const string ColumnCandidateId = "Candidate_Id";
        private const string ColumnApplicantId = "Applicant_Id";

        private readonly IApprenticeshipProgrammeProvider _programmeProvider;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger<ProviderApplicationsReportStrategy> _logger;
        private readonly IApprenticeshipRouteProvider _apprenticeshipRouteProvider;

        private const string QueryFormat = @"[
            { $match: {'trainingProvider.ukprn' : _ukprn_, 'ownerType' : 'Provider', 'isDeleted' : false, 'applicationMethod' : '_applicationMethod_', 'status' : {$in : ['Live','Closed']}, 'vacancyType' : {_vacancytype_ : ['Traineeship']}}},
            { $lookup: { from: 'applicationReviews', localField: 'vacancyReference', foreignField: 'vacancyReference', as: 'ar'}},
            { $unwind: '$ar'},
            { $match: {'ar.submittedDate' : { $gte: ISODate('_fromDate_'), $lte: ISODate('_toDate_')}, 'ar.isWithdrawn' : false}},
            { $project: {
                    '_id' : 0,
                    'Candidate_Name' : { $concat: ['$ar.application.firstName', ' ', '$ar.application.lastName']},
                    'Candidate_Id' : '$ar.application.candidateId',
                    'Address_Line1' : { $ifNull: ['$ar.application.addressLine1', null]},
                    'Address_Line2' : { $ifNull: ['$ar.application.addressLine2', null]},
                    'Address_Line3' : { $ifNull: ['$ar.application.addressLine3', null]},
                    'Address_Line4' : { $ifNull: ['$ar.application.addressLine4', null]},
                    'Postcode' : { $ifNull: ['$ar.application.postcode', null]},
                    'Telephone' : { $ifNull: ['$ar.application.phone', null]},
                    'Email' : { $ifNull: ['$ar.application.email', null]}, 
                    'School' : { $ifNull: ['$ar.application.educationInstitution', null]},
                    'Date_of_Birth' : { $ifNull: ['$ar.application.dateOfBirth', null]},
                    'Vacancy_Reference_Number' : '$vacancyReference',
                    'Vacancy_Title' : '$title',
                    'Programme' : { $ifNull: ['$programmeId', null]},
                    'RouteId' : { $ifNull: ['routeId', null]},
                    'Employer' : { $ifNull: ['$employerName', null]},
                    'Vacancy_Postcode' : { $ifNull: ['$employerLocation.postcode', null]},
                    'Learning_Provider' : { $ifNull: ['$trainingProvider.name', null]},
                    'Application_Date' : { $ifNull: ['$ar.submittedDate', null]},
                    'Vacancy_Closing_Date' : { $ifNull: ['$closingDate', null]},
                    'Application_Status' : { $ifNull: ['$ar.status', null]},
                    'Application_LastUpdatedDate' : { $ifNull: ['$ar.statusUpdatedDate', null]}
                }},
            { $sort : {Vacancy_Reference_Number : 1, Application_Date : 1}}]";

        public ProviderApplicationsReportStrategy(
            ILoggerFactory loggerFactory, 
            IOptions<MongoDbConnectionDetails> details,
            IApprenticeshipProgrammeProvider programmeProvider,
            ITimeProvider timeProvider,
            ILogger<ProviderApplicationsReportStrategy> logger,
            IApprenticeshipRouteProvider apprenticeshipRouteProvider) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
            _programmeProvider = programmeProvider;
            _timeProvider = timeProvider;
            _logger = logger;
            _apprenticeshipRouteProvider = apprenticeshipRouteProvider;
        }

        public Task<ReportStrategyResult> GetReportDataAsync(Dictionary<string,object> parameters)
        {
            var ukprn = long.Parse(parameters[ReportParameterName.Ukprn].ToString());
            var fromDate = (DateTime) parameters[ReportParameterName.FromDate];
            var toDate = (DateTime)parameters[ReportParameterName.ToDate];
            Enum.TryParse<VacancyType>((string) parameters[ReportParameterName.VacancyType], true, out var vacancyType);

            return GetProviderApplicationsAsync(ukprn, fromDate, toDate, vacancyType);
        }

        public ReportDataType ResolveFormat(string fieldName)
        {
            switch (fieldName)
            {
                case "Date_of_Birth":
                case "Application_Date":
                case "Vacancy_Closing_Date":
                    return ReportDataType.DateType;
                default:
                    return ReportDataType.StringType;
            }
        }

        public Task<string> GetApplicationReviewsRecursiveAsync(string queryJson)
        {
            throw new NotImplementedException();
        }

        private async Task<ReportStrategyResult> GetProviderApplicationsAsync(long ukprn, DateTime fromDate, DateTime toDate, VacancyType vacancyType)
        {
            var collection = GetCollection<BsonDocument>();

            var vacancyQuery = "$in";
            var applicationMethod = "ThroughFindATraineeship";
            if (vacancyType == Domain.Entities.VacancyType.Apprenticeship)
            {
                vacancyQuery = "$nin";
                applicationMethod = "ThroughFindAnApprenticeship";
            }

            var queryJson = QueryFormat
                .Replace(QueryUkprn, ukprn.ToString())
                .Replace(QueryFromDate, fromDate.ToString("o"))
                .Replace(QueryToDate , toDate.ToString("o"))
                .Replace(VacancyType, vacancyQuery)
                .Replace(ApplicationMethod, applicationMethod);

            var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument[]>(queryJson);

            var results = await RetryPolicy.Execute(_ =>
                    collection.Aggregate<BsonDocument>(bson).ToListAsync(),
            new Context(nameof(GetProviderApplicationsAsync)));

            await ProcessResultsAsync(results, vacancyType);

            _logger.LogInformation($"Report parameters ukprn:{ukprn} fromDate:{fromDate} toDate:{toDate} returned {results.Count} results");

            var dotNetFriendlyResults = results.Select(BsonTypeMapper.MapToDotNetValue);
            var data = JsonConvert.SerializeObject(dotNetFriendlyResults);

            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Date", _timeProvider.Now.ToUkTime().ToString("dd/MM/yyyy HH:mm:ss")),
                new KeyValuePair<string, string>("Total_Number_Of_Applications", results.Count.ToString())
            };
            return new ReportStrategyResult(headers, data,"");
        }

        private async Task ProcessResultsAsync(List<BsonDocument> results, VacancyType vacancyType)
        {
            foreach (var result in results)
            {
                if (vacancyType == Domain.Entities.VacancyType.Apprenticeship)
                {
                    await SetProgrammeAsync(result);
                }
                else
                {
                    await SetRoute(result);
                }
                
                SetNumberOfDaysAtThisStatus(result);
                SetVacancyReference(result);
                SetApplicantId(result);
            }
        }

        private async Task SetRoute(BsonDocument result)
        {
            var routeId = result[ColumnRoute].AsString;
            var routeName = "";
            if (!string.IsNullOrEmpty(routeId) && int.TryParse(routeId, out var routeIdResult))
            {
                var route = await _apprenticeshipRouteProvider.GetApprenticeshipRouteAsync(routeIdResult);
                routeName = route?.Route;
            }
            
            result.InsertAt(result.IndexOfName(ColumnRoute),
                new BsonElement(ColumnRouteName, (BsonValue)routeName ?? BsonNull.Value));    
            
            result.Remove(ColumnRoute);
        }
        private async Task SetProgrammeAsync(BsonDocument result)
        {    
            var programmeId = result[ColumnProgramme].AsString;
            
            if (string.IsNullOrEmpty(programmeId))
            {
                result.InsertAt(result.IndexOfName(ColumnProgramme),
                    new BsonElement(ColumnStandard, BsonNull.Value));

                result.InsertAt(result.IndexOfName(ColumnProgramme),
                    new BsonElement(ColumnStandardStatus, BsonNull.Value));
                result.Remove(ColumnProgramme);
                return;
            }
            var programme = await _programmeProvider.GetApprenticeshipProgrammeAsync(programmeId);

            var programmeValue = $"{programme.Id} {programme.Title}";
            var programmeStatus = programme.IsActive ? "Active" : "Inactive";

            string framework = null;
            string frameworkStatus = null;
            string standard = null;
            string standardStatus = null;

            if (programme.ApprenticeshipType == TrainingType.Framework)
            {
                framework = programmeValue;
                frameworkStatus = programmeStatus;
            }

            if (programme.ApprenticeshipType == TrainingType.Standard)
            {
                standard = programmeValue;
                standardStatus = programmeStatus;
            }

            result.InsertAt(result.IndexOfName(ColumnProgramme),
                new BsonElement(ColumnFramework, (BsonValue)framework ?? BsonNull.Value));

            result.InsertAt(result.IndexOfName(ColumnProgramme),
                new BsonElement(ColumnFrameworkStatus, (BsonValue)frameworkStatus ?? BsonNull.Value));

            result.InsertAt(result.IndexOfName(ColumnProgramme),
                new BsonElement(ColumnStandard, (BsonValue)standard ?? BsonNull.Value));

            result.InsertAt(result.IndexOfName(ColumnProgramme),
                new BsonElement(ColumnStandardStatus, (BsonValue)standardStatus ?? BsonNull.Value));

            result.Remove(ColumnProgramme);
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

        private void SetVacancyReference(BsonDocument result)
        {
            result[ColumnVacancyReference] = $"VAC{result[ColumnVacancyReference]}";
        }

        private void SetApplicantId(BsonDocument result)
        {
            var candidateId = result[ColumnCandidateId].AsGuid;
            var applicantId = candidateId.ToString().Replace("-", "").Substring(0, 7).ToUpperInvariant();

            result.InsertAt(result.IndexOfName(ColumnCandidateId),
                new BsonElement(ColumnApplicantId, applicantId));
            result.Remove(ColumnCandidateId);
        }
    }
}
