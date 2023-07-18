using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Extensions.Logging;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class GetTrainingProviderDetails : IGetTrainingProviderDetails
    {
        private readonly ILogger<GetTrainingProviderDetails> _logger;
        private readonly IOuterApiClient _outerApiClient;

        public GetTrainingProviderDetails(
            ILogger<GetTrainingProviderDetails> logger,
            IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<GetProviderResponse> GetTrainingProvider(long ukprn)
        {
            try
            {
                var providerTask = await GetProviderDetails(ukprn);
                return providerTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get provider details from api");
                throw;
            }
        }

        private async Task<GetProviderResponse> GetProviderDetails(long ukprn)
        {
            _logger.LogTrace("Getting Provider Details from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.Execute(context => _outerApiClient.Get<GetProviderResponse>(new GetProviderRequest(ukprn)), new Dictionary<string, object>() { { "apiCall", "Providers" } });

            return result;
        }

        private Polly.Retry.RetryPolicy GetApiRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(new[]
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
