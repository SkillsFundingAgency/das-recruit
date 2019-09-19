using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Microsoft.Extensions.Logging;
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
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return GetEsfaTestTrainingProvider();

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