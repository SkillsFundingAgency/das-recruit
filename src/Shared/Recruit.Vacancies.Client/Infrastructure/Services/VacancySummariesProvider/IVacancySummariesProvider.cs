using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public interface IVacancySummariesProvider
    {
        Task<(IList<VacancySummary>, int? totalCount)> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, int page, FilteringOptions? status, string searchTerm);
        Task<(IList<VacancySummary>, int? totalCount)> GetEmployerOwnedVacancySummariesByEmployerAccountId(string employerAccountId, int page, FilteringOptions? status, string searchTerm);
        Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn);
        Task<VacancyDashboard> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, bool ignoreGetApplicationReview);
        Task<VacancyDashboard> GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(string employerAccountId, bool ignoreGetApplicationReview);
        Task<long> VacancyCount(long? ukprn, string employerAccountId, FilteringOptions? filteringOptions, string searchTerm, OwnerType ownerType);
    }
}