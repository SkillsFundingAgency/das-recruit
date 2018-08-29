using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId);
        Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
        Task<IEnumerable<LiveVacancy>> GetLiveVacancies();
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<QaDashboard> GetQaDashboardAsync();
    }
}