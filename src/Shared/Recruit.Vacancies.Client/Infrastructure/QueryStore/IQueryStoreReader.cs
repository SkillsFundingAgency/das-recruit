using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId);
        Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
        Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn);
        Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<QaDashboard> GetQaDashboardAsync();
        Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn);
    }
}