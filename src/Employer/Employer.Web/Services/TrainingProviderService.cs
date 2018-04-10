using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
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

        public async Task<TrainingProvider> GetProviderAsync(long ukprn)
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

        public async Task<bool> ExistsAsync(long ukprn)
        {
            try
            {
                return await _providerClient.ExistsAsync(ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve provider information for UKPRN: {ukprn}");
                throw;
            }
        }
    }
}