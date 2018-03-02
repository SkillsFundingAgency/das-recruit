using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Jobs.Models;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class StandardsAndFrameworksUpdater
    {
        private readonly ILogger<StandardsAndFrameworksUpdater> _logger;
        private readonly IUpdateQueryStore _queryStore;
        private readonly IStandardApiClient _standardsClient;
        private readonly IFrameworkApiClient _frameworksClient;

        public StandardsAndFrameworksUpdater(
            ILogger<StandardsAndFrameworksUpdater> logger, 
            IUpdateQueryStore queryStore,
            IStandardApiClient standardsClient,
            IFrameworkApiClient frameworksClient)
        {
            _logger = logger;
            _queryStore = queryStore;
            _standardsClient = standardsClient;
            _frameworksClient = frameworksClient;
        }

        public async Task UpdateAsync()
        {
            var standards = GetStandards();
            var frameworks = GetFrameworks();
            List<Task> tasks = new List<Task>{ standards, frameworks };

            try
            {
                Task.WaitAll(tasks.ToArray());

                List<ApprenticeshipProgramme> newList = new List<ApprenticeshipProgramme>();
                newList.AddRange(standards.Result);
                newList.AddRange(frameworks.Result);

                var view = new ApprenticeshipProgrammeView
                {
                    ApprenticeshipProgrammes = newList
                };
                    
                await UpdateQueryStore(view);
            }
            catch (AggregateException)
            {
                if (standards.Exception != null)
                {
                    _logger.LogError(standards.Exception, "Failed to get standards from api");
                }

                if (frameworks
                .Exception != null)
                {
                    _logger.LogError(frameworks.Exception, "Failed to get frameworks from api");
                }

                throw;
            }
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetStandards()
        {
            var retryPolicy = GetApiRetryPolicy();

            var standards = await retryPolicy.ExecuteAsync(() => _standardsClient.GetAllAsync());
            
            return standards.FilterAndMapToApprenticeshipProgrammes();
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetFrameworks()
        {
            var retryPolicy = GetApiRetryPolicy();

            var frameworks = await retryPolicy.ExecuteAsync(() => _frameworksClient.GetAllAsync());
            
            return frameworks.FilterAndMapToApprenticeshipProgrammes();
        }

        private async Task UpdateQueryStore(ApprenticeshipProgrammeView view)
        {
            await _queryStore.UpdateStandardsAndFrameworksAsyc(view);
        }
        
        private Polly.Retry.RetryPolicy GetApiRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(new[]
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
