﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public interface ITrainingProviderService
    {
        Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn);
        Task<IEnumerable<Domain.Entities.TrainingProvider>> FindAllAsync();

        /// <summary>
        /// Contract to get the details of Provider from Outer API by given ukprn number.
        /// </summary>
        /// <param name="ukprn">ukprn number.</param>
        /// <returns></returns>
        Task<GetProviderResponseItem> GetProviderDetails(long ukprn);

        /// <summary>
        /// Contract to get the application review stats from outer api by given ukprn number.
        /// </summary>
        /// <param name="ukprn"></param>
        /// <param name="vacancyReferences"></param>
        /// <returns></returns>
        Task<GetApplicationReviewStatsResponse> GetProviderDashboardApplicationReviewStats(long ukprn,
            List<long> vacancyReferences);
        
        /// <summary>
        /// Contract to get the dashboard stats from outer api by given ukprn number.
        /// </summary>
        /// <param name="ukprn"></param>
        /// <returns></returns>
        Task<GetDashboardCountApiResponse> GetProviderDashboardStats(long ukprn);
        
        
        /// <summary>
        /// Contract to get the vacancies under a application review statuses.
        /// </summary>
        /// <returns></returns>
        Task<GetVacanciesDashboardResponse> GetProviderDashboardVacanciesByApplicationReviewStatuses(long ukprn,
            List<ApplicationReviewStatus> vacancyReferences, int pageNumber);
    }
}