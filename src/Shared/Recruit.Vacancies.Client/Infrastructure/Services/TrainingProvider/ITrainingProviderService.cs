using System.Collections.Generic;
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
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<GetProviderDashboardApiResponse> GetProviderDashboardStats(long ukprn, string userId);
        
        
        /// <summary>
        /// Contract to get the vacancies under an application review statuses.
        /// </summary>
        /// <returns></returns>
        Task<GetVacanciesDashboardResponse> GetProviderDashboardVacanciesByApplicationReviewStatuses(long ukprn,
            List<ApplicationReviewStatus> vacancyReferences, int pageNumber, int pageSize);

        Task<IEnumerable<TrainingProviderSummary>> GetCourseProviders(int programmeId);

        /// <summary>
        ///  Contract to get all the vacancies by given filter options.
        /// </summary>
        /// <param name="ukprn"></param>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortOrder"></param>
        /// <param name="filterBy"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        Task<GetVacanciesByUkprnApiResponse> GetProviderVacancies(int ukprn, string userId, int page, int pageSize,
            string sortColumn, string sortOrder, FilteringOptions filterBy, string searchTerm);
    }
}