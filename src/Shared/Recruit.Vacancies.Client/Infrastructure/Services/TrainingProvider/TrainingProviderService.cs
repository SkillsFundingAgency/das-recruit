using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ILogger<TrainingProviderService> _logger;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;


        public TrainingProviderService(ILogger<TrainingProviderService> logger, IReferenceDataReader referenceDataReader, ICache cache, ITimeProvider timeProvider)
        {
            _logger = logger;
            _referenceDataReader = referenceDataReader;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public async Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return GetEsfaTestTrainingProvider();
            
            try
            {
                var providers = await GetProviders();
                _logger.LogInformation($"Returned {providers.Data.Count} providers from cache.");
                var firstProvider = providers.Data.FirstOrDefault();
                _logger.LogInformation($"First provider {firstProvider?.Name} {firstProvider?.Ukprn}");
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
    }
}