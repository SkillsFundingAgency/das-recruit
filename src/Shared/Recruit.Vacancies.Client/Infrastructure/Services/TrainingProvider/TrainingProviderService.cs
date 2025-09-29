using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public class TrainingProviderService(
        ILogger<TrainingProviderService> logger,
        IReferenceDataReader referenceDataReader,
        ICache cache,
        ITimeProvider timeProvider,
        IOuterApiClient outerApiClient)
        : ITrainingProviderService
    {
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
                logger.LogWarning(ex, $"Failed to retrieve provider information for UKPRN: {ukprn}");
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
            logger.LogTrace("Getting Provider Details from Outer Api");

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            var result = await retryPolicy.Execute(_ =>
                    outerApiClient.Get<GetProviderResponseItem>(new GetProviderRequest(ukprn)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });

            return result;
        }

        public async Task<GetApplicationReviewStatsResponse> GetProviderDashboardApplicationReviewStats(long ukprn, List<long> vacancyReferences)
        {
            logger.LogTrace("Getting Provider Application Review Stats from Outer Api");

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Post<GetApplicationReviewStatsResponse>(
                    new GetProviderApplicationReviewsCountApiRequest(ukprn,
                        vacancyReferences)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });
        }

        public async Task<GetProviderDashboardApiResponse> GetProviderDashboardStats(long ukprn, string userId)
        {
            logger.LogTrace("Getting Provider Dashboard Stats from Outer Api");

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetProviderDashboardApiResponse>(
                    new GetProviderDashboardCountApiRequest(ukprn, userId)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });
        }

        public async Task<GetVacanciesByUkprnApiResponse> GetProviderVacancies(int ukprn,
            int page,
            int pageSize,
            string sortColumn,
            string sortOrder,
            FilteringOptions filterBy,
            string searchTerm)
        {
            logger.LogTrace("Getting Provider Vacancies from Outer Api");

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetVacanciesByUkprnApiResponse>(
                    new GetVacanciesByUkprnApiRequest(ukprn, page, pageSize, sortColumn, sortOrder, filterBy, searchTerm)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });
        }


        public async Task<GetVacanciesDashboardResponse> GetProviderDashboardVacanciesByApplicationReviewStatuses(
            long ukprn,
            List<ApplicationReviewStatus> applicationReviewStatusList,
            int pageNumber = 1,
            int pageSize = 25)
        {
            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetVacanciesDashboardResponse>(
                    new GetProviderDashboardVacanciesApiRequest(ukprn, pageNumber, pageSize, applicationReviewStatusList)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });
        }

        public async Task<IEnumerable<TrainingProviderSummary>> GetCourseProviders(int programmeId)
        {
            var result = await outerApiClient.Get<GetCourseProvidersResponse>(new GetCourseProvidersRequest(programmeId));
            return result.Providers?.Select(x => new TrainingProviderSummary
            {
                ProviderName = x.Name,
                Ukprn = x.Ukprn
            }) ?? [];
        }

        public async Task<GetAlertsByUkprnApiResponse> GetProviderAlerts(int ukprn, string userId)
        {
            logger.LogTrace("Getting Provider alerts from Outer Api");

            var retryPolicy = PollyRetryPolicy.GetPolicy();

            return await retryPolicy.Execute(_ => outerApiClient.Get<GetAlertsByUkprnApiResponse>(
                    new GetAlertsByUkprnApiRequest(ukprn, userId)),
                new Dictionary<string, object>
                {
                    {
                        "apiCall", "Providers"
                    }
                });
        }

        private Task<TrainingProviders> GetProviders()
        {
            return cache.CacheAsideAsync(
                CacheKeys.TrainingProviders,
                timeProvider.NextDay6am,
                ()=>referenceDataReader.GetReferenceData<TrainingProviders>());
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