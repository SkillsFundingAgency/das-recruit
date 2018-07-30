using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<Dashboard> GetDashboardAsync(string employerAccountId);
        Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammesAsync();
        Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
        Task<IEnumerable<LiveVacancy>> GetLiveVacancies();
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<QaDashboard> GetQaDashboardAsync();
    }
}