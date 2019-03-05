using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.Apprenticeships.Api.Client;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammesUpdateService : IApprenticeshipProgrammesUpdateService
    {
        private readonly ILogger<ApprenticeshipProgrammesUpdateService> _logger;
        private readonly IStandardApiClient _standardsClient;
        private readonly IFrameworkApiClient _frameworksClient;
        private readonly IReferenceDataWriter _referenceDataWriter;

        public ApprenticeshipProgrammesUpdateService(
            ILogger<ApprenticeshipProgrammesUpdateService> logger, 
            IStandardApiClient standardsClient,
            IFrameworkApiClient frameworksClient,
            IReferenceDataWriter referenceDataWriter)
        {
            _logger = logger;
            _standardsClient = standardsClient;
            _frameworksClient = frameworksClient;
            _referenceDataWriter = referenceDataWriter;
        }

        public async Task UpdateApprenticeshipProgrammesAsync()
        {
            var standardsTask = GetStandards();
            var frameworksTask = GetFrameworks();
            var tasks = new List<Task>{ standardsTask, frameworksTask };

            try
            {
                Task.WaitAll(tasks.ToArray());

                var standardsFromApi = standardsTask.Result.Distinct().ToList();
                var frameworksFromApi = frameworksTask.Result.Distinct().ToList();

                if (standardsFromApi.Count == 0)
                    throw new InfrastructureException("Retrieved 0 standards from the apprenticeships api.");
                
                if (frameworksFromApi.Count == 0)
                    throw new InfrastructureException("Retrieved 0 frameworks from the apprenticeships api.");

                var newList = new List<ApprenticeshipProgramme>();
                newList.AddRange(standardsFromApi);
                newList.AddRange(frameworksFromApi);

                await _referenceDataWriter.UpsertReferenceData(new ApprenticeshipProgrammes
                {
                    Data = newList.Distinct(new ApprenticeshipProgrammeEqualityComparer()).ToList()
                });

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
            _logger.LogTrace("Getting Standards from Apprenticeships Api");

            var retryPolicy = GetApiRetryPolicy();

            var standards = await retryPolicy.ExecuteAsync(context => _standardsClient.GetAllAsync(), new Dictionary<string, object>() {{ "apiCall", "Standards" }});

            return standards.FilterAndMapToApprenticeshipProgrammes();
        }

        private async Task<IEnumerable<ApprenticeshipProgramme>> GetFrameworks()
        {
            _logger.LogTrace("Getting Frameworks from Apprenticeships Api");
            
            var retryPolicy = GetApiRetryPolicy();

            var frameworks = await retryPolicy.ExecuteAsync(context => _frameworksClient.GetAllAsync(), new Dictionary<string, object>() {{ "apiCall", "Frameworks" }});
            
            return frameworks.FilterAndMapToApprenticeshipProgrammes();
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