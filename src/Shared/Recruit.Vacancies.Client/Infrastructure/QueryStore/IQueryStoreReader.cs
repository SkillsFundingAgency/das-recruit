using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId);
        Task<EmployerEditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
        Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn);
        Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<QaDashboard> GetQaDashboardAsync();
        Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn, VacancyType vacancyType);
        Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
        Task<BlockedProviderOrganisations> GetBlockedProvidersAsync();
    }
}