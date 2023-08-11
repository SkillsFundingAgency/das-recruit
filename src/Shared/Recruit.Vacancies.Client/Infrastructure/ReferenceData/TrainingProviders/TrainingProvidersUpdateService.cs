using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            try
            {
                var providersTask = await GetMainAndEmployerProviders();

                await _referenceDataWriter.UpsertReferenceData(new TrainingProviders {
                    Data = providersTask.ToList()
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get providers from api");
                throw;
            }
        }

        [assembly: InternalsVisibleTo("Esfa.Recruit.Employer.UnitTests")]
        internal async Task<IEnumerable<TrainingProvider>> GetMainAndEmployerProviders()
        {
            _logger.LogTrace("Getting All Main and Employer Providers from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.Execute(context => _outerApiClient.Get<GetTrainingProvidersResponse>(new GetTrainingProvidersRequest()), new Dictionary<string, object>() { { "apiCall", "Providers" } });

            _logger.LogTrace("Sample Data count from Remote API" + result.Providers.Count());
            _logger.LogTrace("Sample Data from Remote API" + JsonConvert.SerializeObject(result.Providers.FirstOrDefault()));

            return result.Providers
                .Where(fil =>
                    fil.ProviderTypeId.Equals((short)ProviderTypeIdentifier.MainProvider) ||
                    fil.ProviderTypeId.Equals((short)ProviderTypeIdentifier.EmployerProvider))
                .Select(c => new TrainingProvider
                {
                    Name = c.Name,
                    Ukprn = c.Ukprn,
                    Address = new TrainingProviderAddress
                    {
                        AddressLine1 = c.Address?.Address1,
                        AddressLine2 = c.Address?.Address2,
                        AddressLine3 = c.Address?.Address3,
                        AddressLine4 = c.Address?.Address4,
                    }
                });
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