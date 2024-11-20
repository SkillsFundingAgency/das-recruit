using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal class MongoDbReportRepository : MongoDbCollectionBase, IReportRepository
    {
        private const string OwnerTypeFieldName = "owner.ownerType";
        private const string OwnerUkprnFieldName = "owner.ukprn";
        private const string ParametersVacancyTypeFieldName = "parameters.VacancyType";

        public MongoDbReportRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Reports, details)
        {
        }

        public Task CreateAsync(Report report)
        {
            var collection = GetCollection<Report>();
            return RetryPolicy.Execute(_ => 
                collection.InsertOneAsync(report), 
                new Context(nameof(CreateAsync)));
        }

        public async Task UpdateAsync(Report report)
        {
            var filter = Builders<Report>.Filter.Eq(v => v.Id, report.Id);
            var collection = GetCollection<Report>();
            await RetryPolicy.Execute(_ =>
                    collection.ReplaceOneAsync(filter, report),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<List<T>> GetReportsForProviderAsync<T>(long ukprn)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(OwnerTypeFieldName, ReportOwnerType.Provider.ToString()) &
                         builder.Eq(ParametersVacancyTypeFieldName, VacancyType.Apprenticeship.ToString()) &
                         builder.Eq(OwnerUkprnFieldName, ukprn);

            var collection = GetCollection<T>();

            var result = await RetryPolicy.Execute(_ =>
                    collection.Find(filter)
                        .Project<T>(GetProjection<T>())
                        .ToListAsync(),
                new Context(nameof(GetReportsForProviderAsync)));

            return result;
        }

        public async Task<List<T>> GetReportsForQaAsync<T>()
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(OwnerTypeFieldName, ReportOwnerType.Qa.ToString());

            var collection = GetCollection<T>();

            var result = await RetryPolicy.Execute(_ =>
                    collection.Find(filter)
                        .Project<T>(GetProjection<T>())
                        .ToListAsync(),
                new Context(nameof(GetReportsForProviderAsync)));

            return result;
        }

        public async Task<Report> GetReportAsync(Guid reportId)
        {
            var filter = Builders<Report>.Filter.Eq(r => r.Id, reportId);
            var collection = GetCollection<Report>();

            var result = await RetryPolicy.Execute(async _ =>
                    await collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetReportAsync)));

            return result;
        }

        public async Task<int> DeleteReportsCreatedBeforeAsync(DateTime requestedOn)
        {
            var filter = Builders<Report>.Filter.Lt(r => r.RequestedOn, requestedOn);
            var collection = GetCollection<Report>();

            var result = await RetryPolicy.Execute(async _ =>
                    await collection.DeleteManyAsync(filter),
                new Context(nameof(GetReportAsync)));

            return (int)result.DeletedCount;
        }

        public Task IncrementReportDownloadCountAsync(Guid reportId)
        {
            var filter = Builders<Report>.Filter.Eq(r => r.Id, reportId);
            var update = new UpdateDefinitionBuilder<Report>().Inc(r => r.DownloadCount, 1);
            var collection = GetCollection<Report>();

            return RetryPolicy.Execute(async _ =>
                await collection.FindOneAndUpdateAsync(filter, update),
            new Context(nameof(IncrementReportDownloadCountAsync)));
        }
    }
}