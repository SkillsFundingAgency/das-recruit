using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal class MongoDbReportRepository : MongoDbCollectionBase, IReportRepository
    {
        public MongoDbReportRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Reports, details)
        {
        }

        public Task CreateAsync(Report report)
        {
            var collection = GetCollection<Report>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.InsertOneAsync(report), 
                new Context(nameof(CreateAsync)));
        }
    }
}