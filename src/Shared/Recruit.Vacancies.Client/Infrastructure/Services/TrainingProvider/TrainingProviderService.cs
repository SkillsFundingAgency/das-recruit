using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly ILogger<TrainingProviderService> _logger;
        private readonly IProviderApiClient _providerClient;


        public TrainingProviderService(ILogger<TrainingProviderService> logger, IProviderApiClient providerClient)
        {
            _logger = logger;
            _providerClient = providerClient;
        }

        public async Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn)
        {
            Provider provider;

            try
            {
                provider = await _providerClient.GetAsync(ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve provider information for UKPRN: {ukprn}");
                throw;
            }

            return TrainingProviderMapper.MapFromApiProvider(provider);
        }

        public async Task<IEnumerable<TrainingProviderSuggestion>> FindAllAsync()
        {
            var response = await _providerClient.FindAllAsync();

            return response.Select(r => new TrainingProviderSuggestion
            {
                Ukprn = r.Ukprn,
                ProviderName = r.ProviderName
            });
        }
    }
}