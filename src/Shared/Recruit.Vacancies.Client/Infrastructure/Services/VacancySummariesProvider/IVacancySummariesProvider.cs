using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Azure.ServiceBus;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public interface IVacancySummariesProvider
    {
        Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountAsync(string employerAccountId);
        Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesInReviewByEmployerAccountAsync(string employerAccountId);
        Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn, VacancyType vacancyType, int page, FilteringOptions? status, string searchTerm);
        Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn, VacancyType vacancyType);
        Task<IList<VacancyDashboard>> GetProviderOwnedVacancyDashboardByUkprnAsync(long ukprn, VacancyType vacancyType);
        Task<long> VacancyCount(long ukprn, VacancyType vacancyType, FilteringOptions? filteringOptions);
    }
}