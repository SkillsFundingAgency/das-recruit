using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using SFA.DAS.Providers.Api.Client;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider
{
    public class TrainingProviderSummaryProvider : ITrainingProviderSummaryProvider
    {
        private readonly ILogger<TrainingProviderSummaryProvider> _logger;
        private readonly IProviderApiClient _providerClient;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;


        public TrainingProviderSummaryProvider(ILogger<TrainingProviderSummaryProvider> logger, IProviderApiClient providerClient, ICache cache, ITimeProvider timeProvider)
        {
            _logger = logger;
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