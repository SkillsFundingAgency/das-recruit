using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class UpdateStandardsAndFrameworksJob
    {
        private readonly ILogger<GenerateVacancyNumberJob> _logger;
        private IApprenticeshipProgrammeApiClient _apiClient;

        public UpdateStandardsAndFrameworksJob(ILogger<GenerateVacancyNumberJob> logger, IApprenticeshipProgrammeApiClient apiClient)
        {
            _logger = logger;
           _apiClient = apiClient;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            _logger.LogInformation("Getting Standards and Frameworks from Apprenticships Api");

            var retryPolicy = GetRetryPolicy();
            ApprenticeshipSummary results = null;
            
            try
            {
                await retryPolicy.ExecuteAsync(
                    async () =>
                    {
                        results = await _apiClient.GetAsync();
                    });
                
                //UpdateQueryStore(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get standards and frameworks from apprenticeship api.");
            }
        }

        private void UpdateQueryStore(IEnumerable<ApprenticeshipSummary> results)
        {
            // Upsert document in Cosmos store
            throw new NotImplementedException();
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning($"Error connecting to Apprenticeships Api. Retrying...attempt: {retryCount}");    
                    });
        }
    }
}