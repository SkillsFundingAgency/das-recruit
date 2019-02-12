using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

#if DEBUG
namespace Esfa.Recruit.Vacancies.Jobs.SpikeJob
{
    public class SpikeJob
    {
        private readonly ILogger<SpikeJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IVacancySummariesProvider _query;

        public SpikeJob(ILogger<SpikeJob> logger, RecruitWebJobsSystemConfiguration jobsConfig, IVacancySummariesProvider query)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _query = query;
        }

        public async Task CallAggregate([QueueTrigger("test-queue", Connection = "QueueStorage")] string message, TextWriter log)
        {
            var employerAccountId = Environment.GetEnvironmentVariable("EmployerAccount");
            var result = await _query.GetEmployerOwnedVacancySummariesByEmployerAccountAsync(employerAccountId);
        }
    }
}
#endif