﻿using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.Providers.Api.Client;
using System;
using System.Threading.Tasks;

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
                _logger.LogInformation(ex, $"Failed to retrieve provider information for UKPRN: {ukprn}");
                return null;
            }

            return TrainingProviderMapper.MapFromApiProvider(provider);
        }
    }
}