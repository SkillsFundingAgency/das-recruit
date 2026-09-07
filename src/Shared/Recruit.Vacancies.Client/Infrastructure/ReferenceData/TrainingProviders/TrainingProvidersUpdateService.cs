using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.Extensions.Logging;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders
{
    public class TrainingProvidersUpdateService(
        ILogger<TrainingProvidersUpdateService> logger,
        IReferenceDataWriter referenceDataWriter,
        IOuterApiClient outerApiClient)
        : ITrainingProvidersUpdateService
    {
        public async Task UpdateProviders()
        {
            try
            {
                var providersTask = await GetProviders();

                await referenceDataWriter.UpsertReferenceData(new TrainingProviders {
                    Data = providersTask.ToList()
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get providers from api");
                throw;
            }
        }

        private async Task<IEnumerable<TrainingProvider>> GetProviders()
        {
            logger.LogTrace("Getting Providers from Outer Api");

            var retryPolicy = GetApiRetryPolicy();

            var result = await retryPolicy.Execute(context => outerApiClient.Get<GetProvidersResponse>(new GetProvidersRequest()), new Dictionary<string, object> { { "apiCall", "Providers" } });

            // logic to filter only Training provider with Main & Employer Profiles and Status Id not equal to "Not Currently Starting New Apprentices"
            return result.Providers
                .Where(fil =>
                    (fil.ProviderTypeId.Equals((short)ProviderTypeIdentifier.MainProvider) ||
                    fil.ProviderTypeId.Equals((short)ProviderTypeIdentifier.EmployerProvider)) && 
                    !fil.StatusId.Equals((short)ProviderStatusType.ActiveButNotTakingOnApprentices))
                .Select(c => (TrainingProvider)c);
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
                }, (_, timeSpan, retryCount, context) => {
                    logger.LogWarning("Error connecting to Outer Api for {O}. Retrying in {TimeSpanSeconds} secs...attempt: {RetryCount}", context["apiCall"], timeSpan.Seconds, retryCount);    
                });
        }
    }
}