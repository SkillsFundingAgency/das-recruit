using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Apprenticeships.Api.Client;

namespace Esfa.Recruit.Vacancies.Jobs.TrainingTypes
{
    public sealed class StandardsAndFrameworksUpdater
    {
        private readonly ILogger<StandardsAndFrameworksUpdater> _logger;
        private readonly IVacancyClient _queryStoreClient;
        private readonly IStandardApiClient _standardsClient;
        private readonly IFrameworkApiClient _frameworksClient;

        public StandardsAndFrameworksUpdater(
            ILogger<StandardsAndFrameworksUpdater> logger, 
            IVacancyClient queryStoreClient,
            IStandardApiClient standardsClient,
            IFrameworkApiClient frameworksClient)
        {
            _logger = logger;
            _queryStoreClient = queryStoreClient;
            _standardsClient = standardsClient;
            _frameworksClient = frameworksClient;
        }

        public async Task UpdateAsync()
        {
            var standardsTask = GetStandards();
            var frameworksTask = GetFrameworks();
            List<Task> tasks = new List<Task>{ standardsTask, frameworksTask };

            try
            {
                Task.WaitAll(tasks.ToArray());

                var standardsFromApi = standardsTask.Result.ToList();
                var frameworksFromApi = frameworksTask.Result.ToList();

                List<ApprenticeshipProgramme> newList = new List<ApprenticeshipProgramme>();
                newList.AddRange(standardsFromApi);
                newList.AddRange(frameworksFromApi);

                await UpdateQueryStore(newList);

                _logger.LogInformation("Inserted: {standardCount} standards and {frameworkCount} frameworks.", standardsFromApi.Count, frameworksFromApi.Count);
            }
            catch (AggregateException)
            {
                if (standardsTask.Exception != null)
                {
                    _logger.LogError(standardsTask.Exception, "Failed to get standards from api");
                }

                if (frameworksTask.Exception != null)
                {
                    _logger.LogError(frameworksTask.Exception, "Failed to get frameworks from api");
                }

                throw;
            }
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetStandards()
        {
            _logger.LogTrace("Getting Standards from Apprentieships Api");

            var retryPolicy = GetApiRetryPolicy();

            var standards = await retryPolicy.ExecuteAsync(() => _standardsClient.GetAllAsync(), new Dictionary<string, object>() {{ "apiCall", "Standards" }});

            return standards.FilterAndMapToApprenticeshipProgrammes();
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetFrameworks()
        {
            _logger.LogTrace("Getting Frameworks from Apprentieships Api");
            
            var retryPolicy = GetApiRetryPolicy();

            var frameworks = await retryPolicy.ExecuteAsync(() => _frameworksClient.GetAllAsync(), new Dictionary<string, object>() {{ "apiCall", "Frameworks" }});
            
            return frameworks.FilterAndMapToApprenticeshipProgrammes();
        }

        private async Task UpdateQueryStore(IEnumerable<ApprenticeshipProgramme> programmes)
        {
            await _queryStoreClient.UpdateApprenitceshipProgrammesAsync(programmes);
        }
        
        private Polly.Retry.RetryPolicy GetApiRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning($"Error connecting to Apprenticeships Api for {context["apiCall"]}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");    
                    });
        }
    }
}
