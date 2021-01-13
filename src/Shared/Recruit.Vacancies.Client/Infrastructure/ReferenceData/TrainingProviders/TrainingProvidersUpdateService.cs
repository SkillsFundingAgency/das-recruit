using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Microsoft.Extensions.Logging;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders
{
    public class TrainingProvidersUpdateService : ITrainingProvidersUpdateService
    {
        private readonly ILogger<TrainingProvidersUpdateService> _logger;
        private readonly IReferenceDataWriter _referenceDataWriter;
        private readonly IOuterApiClient _outerApiClient;

        public TrainingProvidersUpdateService(
            ILogger<TrainingProvidersUpdateService> logger, 
            IReferenceDataWriter referenceDataWriter, 
            IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _referenceDataWriter = referenceDataWriter;
            _outerApiClient = outerApiClient;
        }
        
        public async Task UpdateProviders()
        {
            var providersTask = GetProviders();
            try
            {
                Task.WaitAll(providersTask);
                
                await _referenceDataWriter.UpsertReferenceData(new TrainingProviders {
                    Data = providersTask.Result.ToList()
                });
            }
            catch (AggregateException)
            {
                if (providersTask.Exception != null)
                {
                    _logger.LogError(providersTask.Exception, "Failed to get providers from api");
                }

                throw;
            }
        }
        
        private async Task<IEnumerable<TrainingProvider>> GetProviders()
        {
            _logger.LogTrace("Getting Providers from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.ExecuteAsync(context => _outerApiClient.Get<GetProvidersResponse>(new GetProvidersRequest()), new Dictionary<string, object>() {{ "apiCall", "Providers" }});

            return result.Providers.Select(c=>(TrainingProvider)c);

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
                    _logger.LogWarning($"Error connecting to Outer Api for {context["apiCall"]}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");    
                });
        }
    }
}