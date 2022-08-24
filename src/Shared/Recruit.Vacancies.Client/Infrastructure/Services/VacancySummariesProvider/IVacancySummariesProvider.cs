using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public interface IVacancySummariesProvider
    {
        Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, VacancyType vacancyType, int page, FilteringOptions? status, string searchTerm);
        Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountId(string employerAccountId, VacancyType vacancyType, int page, FilteringOptions? status, string searchTerm);
        Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn, VacancyType vacancyType);
        Task<IList<VacancyDashboard>> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, VacancyType vacancyType);
        Task<IList<VacancyDashboard>> GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(string employerAccountId, VacancyType vacancyType);
        Task<long> VacancyCount(long? ukprn, string employerAccountId, VacancyType vacancyType, FilteringOptions? filteringOptions, string searchTerm, OwnerType ownerType);
    }
}