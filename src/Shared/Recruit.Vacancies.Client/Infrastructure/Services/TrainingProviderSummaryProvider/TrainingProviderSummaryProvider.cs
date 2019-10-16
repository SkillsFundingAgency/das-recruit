using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Providers.Api.Client;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider
{
    /// <summary>
    /// Returns providers from RoATP (Register of Apprenticeship Training Providers)
    /// </summary>
    public class TrainingProviderSummaryProvider : ITrainingProviderSummaryProvider
    {
        private readonly IProviderApiClient _providerClient;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;

        public TrainingProviderSummaryProvider(IProviderApiClient providerClient, ICache cache, ITimeProvider timeProvider)
        {
            _providerClient = providerClient;
            _cache = cache;
            _timeProvider = timeProvider;
        }

        public Task<IEnumerable<TrainingProviderSummary>> FindAllAsync()
        {
            return _cache.CacheAsideAsync(
                CacheKeys.TrainingProviders,
                _timeProvider.NextDay6am,
                FindAllInternalAsync);
        }

        public async Task<TrainingProviderSummary> GetAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return new TrainingProviderSummary { Ukprn = EsfaTestTrainingProvider.Ukprn, ProviderName = EsfaTestTrainingProvider.Name };

            return (await FindAllAsync()).SingleOrDefault(p => p.Ukprn == ukprn);
        }

        private async Task<IEnumerable<TrainingProviderSummary>> FindAllInternalAsync()
        {
            var response = await _providerClient.FindAllAsync();

            return response.Select(r => new TrainingProviderSummary
            {
                Ukprn = r.Ukprn,
                ProviderName = r.ProviderName
            });
        }
    }
}