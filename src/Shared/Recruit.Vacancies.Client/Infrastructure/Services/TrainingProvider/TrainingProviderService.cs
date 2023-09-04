using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Microsoft.Extensions.Logging;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ILogger<TrainingProviderService> _logger;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;
        private readonly IOuterApiClient _outerApiClient;


        public TrainingProviderService(ILogger<TrainingProviderService> logger, IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider, IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
            _outerApiClient = outerApiClient;
        }

        public async Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return GetEsfaTestTrainingProvider();
            
            try
            {
                var providers = await GetProviders();
                var provider = providers.Data.SingleOrDefault(c=>c.Ukprn == ukprn);
                return TrainingProviderMapper.MapFromApiProvider(provider);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to retrieve provider information for UKPRN: {ukprn}");
                return null;
            }
        }

        public async Task<IEnumerable<Domain.Entities.TrainingProvider>> FindAllAsync()
        {
            var providers = await GetProviders();
            return providers.Data.Select(TrainingProviderMapper.MapFromApiProvider).ToList();
        }

        /// <inheritdoc />
        public async Task<GetProviderResponseItem> GetProviderDetails(long ukprn)
        {
            _logger.LogTrace("Getting Provider Details from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.Execute(context => _outerApiClient.Get<GetProviderResponseItem>(new GetProviderRequest(ukprn)), new Dictionary<string, object>() { { "apiCall", "Providers" } });

            return result;
        }

        private Task<TrainingProviders> GetProviders()
        {
            return _cache.CacheAsideAsync(
                CacheKeys.TrainingProviders,
                _timeProvider.NextDay6am,
                ()=>_referenceDataReader.GetReferenceData<TrainingProviders>());
        }

        private Domain.Entities.TrainingProvider GetEsfaTestTrainingProvider()
        {
            return new Domain.Entities.TrainingProvider
            {
                Ukprn = EsfaTestTrainingProvider.Ukprn,
                Name = EsfaTestTrainingProvider.Name,
                Address = new Domain.Entities.Address
                {
                    AddressLine1 = EsfaTestTrainingProvider.AddressLine1,
                    AddressLine2 = EsfaTestTrainingProvider.AddressLine2,
                    AddressLine3 = EsfaTestTrainingProvider.AddressLine3,
                    AddressLine4 = EsfaTestTrainingProvider.AddressLine4,
                    Postcode = EsfaTestTrainingProvider.Postcode
                }
            };
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