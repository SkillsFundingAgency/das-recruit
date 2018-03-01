using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class UpdateStandardsAndFrameworksJob
    {
        private readonly ILogger<GenerateVacancyNumberJob> _logger;

        public UpdateStandardsAndFrameworksJob(ILogger<GenerateVacancyNumberJob> logger)
        {
            _logger = logger;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Using logger to log......");
            Console.WriteLine("We're doing something");
            await Task.CompletedTask;
        }

    }
}